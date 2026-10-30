import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manual-usuario',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manual-usuario.component.html',
  styleUrl: './manual-usuario.component.scss'
})
export class ManualUsuarioComponent implements AfterViewInit, OnDestroy {
  @ViewChild('sidebar') sidebarRef!: ElementRef<HTMLElement>;
  @ViewChild('content') contentRef!: ElementRef<HTMLElement>;

  sidebarOpen = false;
  generandoPdf = false;
  activeSection = 'portada';

  private observer: IntersectionObserver | null = null;

  ngAfterViewInit(): void {
    if (!('IntersectionObserver' in window)) return;
    const targets = this.contentRef.nativeElement.querySelectorAll('[data-section]');
    this.observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            this.activeSection = (entry.target as HTMLElement).id;
          }
        }
      },
      { rootMargin: '-15% 0px -70% 0px', threshold: 0 }
    );
    targets.forEach((t) => this.observer!.observe(t));
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  irASeccion(event: Event, id: string): void {
    event.preventDefault();
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  async descargarPdf(): Promise<void> {
    if (this.generandoPdf) return;
    this.generandoPdf = true;
    try {
      const { default: jsPDF } = await import('jspdf');
      const doc = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });
      const elemento = this.contentRef.nativeElement;
      await doc.html(elemento, {
        margin: [24, 24, 24, 24],
        autoPaging: 'text',
        width: 547,
        windowWidth: elemento.scrollWidth
      });
      doc.save('Manual-Usuario-Canal-Etico-Corporativo.pdf');
    } finally {
      this.generandoPdf = false;
    }
  }
}
