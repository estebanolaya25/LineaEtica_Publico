import { Component, inject } from '@angular/core';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-mensaje-modal',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './mensaje-modal.component.html',
  styles: [`
    .modal-wrapper {
      background: #fff;
      border-radius: 20px;
      border-top: 4px solid #486581;
      min-width: 320px;
      max-width: 440px;
      width: 100%;
      box-shadow: 0 24px 64px rgba(72,101,129,0.18), 0 4px 16px rgba(0,0,0,0.10);
      overflow: hidden;
      position: relative;
      padding-top: 0.5rem;
      text-align: center;
    }
    .btn-dismiss {
      position: absolute;
      top: 0.75rem;
      right: 0.75rem;
      background: none;
      border: none;
      color: #aaa;
      font-size: 1rem;
      cursor: pointer;
      line-height: 1;
      padding: 0.25rem;
      transition: color 0.15s;
    }
    .btn-dismiss:hover { color: #486581; }
    .modal-icon-wrap {
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 1.75rem auto 0.75rem;
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: rgba(72,101,129,0.1);
      font-size: 2rem;
      color: #486581;
    }
    .modal-icon-wrap.is-error {
      background: rgba(220,53,69,0.1);
      color: #dc3545;
    }
    .modal-body { padding: 0.5rem 1.75rem 0.25rem; }
    .modal-title { font-size: 1.15rem; font-weight: 700; color: #1a202c; margin-bottom: 0.5rem; }
    .modal-message { color: #718096; font-size: 0.92rem; line-height: 1.55; margin: 0; }
    .modal-footer { padding: 1.25rem 1.75rem 1.5rem; display: flex; justify-content: center; border: none; }
    .btn-cerrar {
      padding: 0.55rem 2rem;
      font-size: 0.95rem;
      font-weight: 600;
      border-radius: 10px;
      border: none;
      color: #fff;
      background: linear-gradient(90deg, #334e68 0%, #486581 100%);
      cursor: pointer;
      transition: transform 0.1s, box-shadow 0.2s, opacity 0.2s;
    }
    .btn-cerrar:hover { transform: translateY(-1px); box-shadow: 0 6px 18px rgba(72,101,129,0.4); opacity: 0.93; }
    .btn-cerrar:active { transform: translateY(0); }
  `]
})
export class MensajeModalComponent {
  dialogRef = inject(DialogRef);
  data = inject<{ name: string; title: string; isError: boolean }>(DIALOG_DATA);
}
