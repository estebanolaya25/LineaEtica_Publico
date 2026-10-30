import { Injectable, inject } from '@angular/core';
import { forkJoin, Observable, of, switchMap, map } from 'rxjs';
import {
  DenunciaService, FormularioDto, ParametroDto,
  RespuestaCampoDto, RespuestaTablaDto, TIPO_CAMPO
} from '../pages/denuncia/denuncia.service';

export interface PdfMeta {
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  esAnonimo: boolean;
  usuarioReportante?: string | null;
  fechaCreacion?: string | null;
}

type RGB = [number, number, number];
interface PdfColors {
  primary:   RGB;
  priDark:   RGB;
  lightGray: RGB;
  lineGray:  RGB;
  textDark:  RGB;
  textMuted: RGB;
  white:     RGB;
  rowAlt:    RGB;
  thBg:      RGB;
}

@Injectable({ providedIn: 'root' })
export class PdfDenunciaService {
  private svc = inject(DenunciaService);

  generarPdfReporter(idDenuncia: number, meta: PdfMeta): Observable<void> {
    return this.svc.getBorrador(idDenuncia).pipe(
      switchMap(borrador =>
        this.cargarFormYParams(borrador.idFormulario).pipe(
          map(({ formulario, parametrosMap }) =>
            this.buildAndDownload(formulario, parametrosMap, borrador.respuestas, borrador.respuestasTabla, meta)
          )
        )
      )
    );
  }

  generarPdfAdmin(idDenuncia: number, meta: PdfMeta): Observable<void> {
    return this.svc.getDenunciaDetalleAdmin(idDenuncia).pipe(
      switchMap(detalle =>
        this.cargarFormYParams(detalle.idFormulario).pipe(
          map(({ formulario, parametrosMap }) =>
            this.buildAndDownload(formulario, parametrosMap, detalle.respuestas, detalle.respuestasTabla, {
              ...meta,
              usuarioReportante: detalle.usuarioReportante
            })
          )
        )
      )
    );
  }

  private cargarFormYParams(idFormulario: number): Observable<{
    formulario: FormularioDto;
    parametrosMap: Map<number, ParametroDto[]>;
  }> {
    return this.svc.getFormulario(idFormulario).pipe(
      switchMap(formulario => {
        const tipos = this.getTiposParametro(formulario);
        if (tipos.length === 0) {
          return of({ formulario, parametrosMap: new Map<number, ParametroDto[]>() });
        }
        const obs: Record<string, Observable<ParametroDto[]>> = {};
        tipos.forEach(t => { obs[t] = this.svc.getParametros(t); });
        return forkJoin(obs).pipe(
          map(res => {
            const parametrosMap = new Map<number, ParametroDto[]>();
            for (const [k, v] of Object.entries(res))
              parametrosMap.set(Number(k), v as ParametroDto[]);
            return { formulario, parametrosMap };
          })
        );
      })
    );
  }

  private getTiposParametro(formulario: FormularioDto): number[] {
    const tipos = new Set<number>();
    for (const s of formulario.secciones)
      for (const c of s.campos) {
        if (c.tipoParametro) tipos.add(c.tipoParametro);
        for (const h of c.camposHijo)
          if (h.tipoParametro) tipos.add(h.tipoParametro);
      }
    return Array.from(tipos);
  }

  private buildAndDownload(
    formulario: FormularioDto,
    parametrosMap: Map<number, ParametroDto[]>,
    respuestas: RespuestaCampoDto[],
    respuestasTabla: RespuestaTablaDto[],
    meta: PdfMeta
  ): void {
    import('jspdf').then(({ default: jsPDF }) => {
      const doc    = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
      const pageW  = 210;
      const pageH  = 297;
      const mL     = 14;
      const mR     = 14;
      const mT     = 15;
      const mB     = 16;
      const cW     = pageW - mL - mR;
      let y        = mT;

      const C: PdfColors = {
        primary:   [72, 101, 129],
        priDark:   [51, 78, 104],
        lightGray: [240, 244, 248],
        lineGray:  [210, 218, 228],
        textDark:  [45, 55, 72],
        textMuted: [113, 128, 150],
        white:     [255, 255, 255],
        rowAlt:    [248, 250, 252],
        thBg:      [220, 230, 240],
      };

      const checkBreak = (needed: number): void => {
        if (y + needed > pageH - mB) { doc.addPage(); y = mT; }
      };

      doc.setFillColor(...C.primary);
      doc.rect(0, 0, pageW, 20, 'F');
      doc.setFillColor(...C.priDark);
      doc.rect(0, 17, pageW, 3, 'F');
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(13);
      doc.setTextColor(...C.white);
      doc.text('CANAL ÉTICO CORPORATIVO — REPORTE DE DENUNCIA', pageW / 2, 12, { align: 'center' });
      y = 27;

      doc.setFillColor(...C.lightGray);
      doc.rect(mL, y, cW, 26, 'F');
      doc.setDrawColor(...C.lineGray);
      doc.setLineWidth(0.3);
      doc.rect(mL, y, cW, 26, 'S');

      const half = cW / 2;
      const metaY = y + 5;
      const col2X = mL + half + 4;

      const metaRow = (label: string, val: string, rowY: number, leftX = mL + 4, col2 = col2X): void => {
        doc.setFontSize(8.5);
        doc.setFont('helvetica', 'bold');
        doc.setTextColor(...C.primary);
        doc.text(`${label}:`, leftX, rowY);
        doc.setFont('helvetica', 'normal');
        doc.setTextColor(...C.textDark);
        const off = doc.getTextWidth(`${label}: `);
        doc.text(val, leftX + off + 1, rowY);
      };

      metaRow('Formulario', meta.nombreFormulario || '—', metaY);
      metaRow('Radicado',   meta.consecutivo ? `#${meta.consecutivo}` : 'Sin radicado', metaY, col2X);
      metaRow('Estado',     meta.estado || '—', metaY + 7);
      const fecha = meta.fechaCreacion
        ? new Date(meta.fechaCreacion).toLocaleDateString('es-CO')
        : '—';
      metaRow('Fecha', fecha, metaY + 7, col2X);
      metaRow('Tipo', meta.esAnonimo ? 'Anónima' : 'No anónima', metaY + 14);
      if (!meta.esAnonimo && meta.usuarioReportante)
        metaRow('Reportante', meta.usuarioReportante, metaY + 14, col2X);

      y += 32;

      for (const seccion of formulario.secciones) {
        const camposRender = seccion.campos.filter(c => c.tipoCampo !== TIPO_CAMPO.LABEL);
        if (camposRender.length === 0) continue;

        checkBreak(16);

        doc.setFillColor(...C.primary);
        doc.rect(mL, y, cW, 7.5, 'F');
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9.5);
        doc.setTextColor(...C.white);
        doc.text(seccion.nombre || `Sección ${seccion.id}`, mL + 4, y + 5.2);
        y += 9.5;

        for (const campo of seccion.campos) {
          if (campo.tipoCampo === TIPO_CAMPO.LABEL) continue;

          const label = campo.nombre || '—';

          if (campo.tipoCampo === TIPO_CAMPO.TABLA) {
            const grupos = this.getGruposTabla(campo.id, respuestasTabla);
            if (grupos.length === 0) continue;

            const rowH = 6;
            const hijos = campo.camposHijo;
            const colCount = hijos.length + 1;
            const colW = cW / colCount;

            checkBreak(8 + rowH * 2 + rowH * grupos.length);

            doc.setFillColor(...C.lightGray);
            doc.rect(mL, y, cW, 6, 'F');
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(8.5);
            doc.setTextColor(...C.textDark);
            doc.text(label, mL + 2, y + 4.2);
            y += 7;

            doc.setFillColor(...C.thBg);
            doc.rect(mL, y, cW, rowH, 'F');
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(7.5);
            doc.setTextColor(...C.primary);
            doc.text('#', mL + 2, y + 4.2);
            hijos.forEach((h, ci) => {
              const txt = (h.nombre || '').length > 22 ? (h.nombre || '').slice(0, 20) + '…' : (h.nombre || '');
              doc.text(txt, mL + colW * (ci + 1) + 2, y + 4.2);
            });
            y += rowH;

            grupos.forEach((grupo, gi) => {
              checkBreak(rowH + 2);
              if (gi % 2 === 0) {
                doc.setFillColor(...C.rowAlt);
                doc.rect(mL, y, cW, rowH, 'F');
              }
              doc.setFont('helvetica', 'normal');
              doc.setFontSize(8);
              doc.setTextColor(...C.textDark);
              doc.text(String(gi + 1), mL + 2, y + 4.2);

              hijos.forEach((h, ci) => {
                const resp = respuestasTabla.find(
                  r => r.idCampoTabla === campo.id && r.idGrupo === grupo && r.idCampoHijo === h.id
                );
                let val = resp?.textoRespuesta ?? '—';
                if (h.tipoParametro && resp?.idOpcion) {
                  val = parametrosMap.get(h.tipoParametro)?.find(p => p.id === resp.idOpcion)?.nombre ?? val;
                }
                if (!val) val = '—';
                const maxCell = Math.floor((colW - 4) / 2.5);
                if (val.length > maxCell) val = val.slice(0, maxCell - 1) + '…';
                doc.text(val, mL + colW * (ci + 1) + 2, y + 4.2);
              });

              doc.setDrawColor(...C.lineGray);
              doc.setLineWidth(0.2);
              doc.line(mL, y + rowH, mL + cW, y + rowH);
              y += rowH;
            });
            y += 4;

          } else if (campo.tipoCampo === TIPO_CAMPO.ADJUNTO) {
            checkBreak(9);
            this.drawField(doc, label, '[Archivo adjunto]', mL, y, cW, C, true);
            y += 8;

          } else {
            let valor = '—';

            if (campo.tipoCampo === TIPO_CAMPO.SELECT_UNICO) {
              const resp = respuestas.find(r => r.idCampo === campo.id);
              if (resp?.idOpcion)
                valor = parametrosMap.get(campo.tipoParametro)?.find(p => p.id === resp.idOpcion)?.nombre ?? '—';
            } else if (campo.tipoCampo === TIPO_CAMPO.SELECT_MULTIPLE) {
              const opts = respuestas.filter(r => r.idCampo === campo.id && r.idOpcion != null);
              if (opts.length > 0) {
                const lista = parametrosMap.get(campo.tipoParametro) ?? [];
                valor = opts.map(o => lista.find(p => p.id === o.idOpcion)?.nombre ?? String(o.idOpcion)).join(', ');
              }
            } else {
              valor = respuestas.find(r => r.idCampo === campo.id)?.textoRespuesta || '—';
            }

            const maxW = cW - 56;
            const lines = doc.splitTextToSize(valor, maxW);
            const fieldH = Math.max(8, lines.length * 5 + 3);

            checkBreak(fieldH);
            this.drawField(doc, label, lines, mL, y, cW, C);
            y += fieldH;
          }
        }

        doc.setDrawColor(...C.lineGray);
        doc.setLineWidth(0.4);
        doc.line(mL, y, mL + cW, y);
        y += 5;
      }

      const total = doc.getNumberOfPages();
      const fechaGen = new Date().toLocaleString('es-CO', { dateStyle: 'short', timeStyle: 'short' });
      for (let p = 1; p <= total; p++) {
        doc.setPage(p);
        doc.setFillColor(...C.lightGray);
        doc.rect(0, pageH - 11, pageW, 11, 'F');
        doc.setDrawColor(...C.lineGray);
        doc.setLineWidth(0.3);
        doc.line(0, pageH - 11, pageW, pageH - 11);
        doc.setFont('helvetica', 'italic');
        doc.setFontSize(7);
        doc.setTextColor(...C.textMuted);
        doc.text(`Generado el: ${fechaGen}`, mL, pageH - 5);
        doc.text(`Página ${p} de ${total}`, pageW - mR, pageH - 5, { align: 'right' });
      }

      const nombre = meta.consecutivo
        ? `denuncia_${meta.consecutivo}.pdf`
        : `denuncia_${new Date().toISOString().slice(0, 10)}.pdf`;
      doc.save(nombre);
    });
  }

  private drawField(
    doc: any,
    label: string,
    value: string | string[],
    x: number, y: number, cW: number,
    C: PdfColors,
    italic = false
  ): void {
    const labelW = 52;
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8.5);
    doc.setTextColor(...C.textMuted);
    const labelTxt = label.length > 28 ? label.slice(0, 26) + '…' : label;
    doc.text(`${labelTxt}:`, x + 2, y + 4.5);

    doc.setFont('helvetica', italic ? 'italic' : 'normal');
    doc.setTextColor(...C.textDark);
    const vals = Array.isArray(value) ? value : [value];
    vals.forEach((line, i) => {
      doc.text(line, x + labelW + 2, y + 4.5 + i * 5);
    });
  }

  private getGruposTabla(idCampoTabla: number, respuestasTabla: RespuestaTablaDto[]): number[] {
    const grupos = new Set<number>();
    respuestasTabla.filter(r => r.idCampoTabla === idCampoTabla).forEach(r => grupos.add(r.idGrupo));
    return Array.from(grupos).sort((a, b) => a - b);
  }
}
