import { Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { Dialog } from '@angular/cdk/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../auth.service';
import { MensajeModalComponent } from '../mensaje-modal/mensaje-modal.component';

function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';
  if (!value) return null;
  const hasMin8 = value.length >= 8;
  const hasUpper = /[A-Z]/.test(value);
  const hasSpecial = /[!-/:-@{-~^_]/.test(value);
  return hasMin8 && hasUpper && hasSpecial ? null : { passwordStrength: true };
}

function passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
  const pass = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pass === confirm ? null : { passwordsMismatch: true };
}

@Component({
  selector: 'app-cambiar-password',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule],
  templateUrl: './cambiar-password.component.html',
  styleUrl: './cambiar-password.component.scss',
})
export class CambiarPasswordComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(Dialog);
  private translate = inject(TranslateService);

  isLoading = false;
  Lang = 'es';

  form: FormGroup = new FormGroup(
    {
      newPassword: new FormControl('', [Validators.required, passwordStrengthValidator]),
      confirmPassword: new FormControl('', [Validators.required]),
    },
    { validators: passwordsMatchValidator }
  );

  ngOnInit(): void {
    this.Lang = localStorage.getItem('language') || 'es';
    this.translate.use(this.Lang);

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
    }
  }

  guardar(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    const { newPassword } = this.form.value;

    this.authService.updatePassword(newPassword).subscribe({
      next: () => {
        this.isLoading = false;
        const dialogRef = this.dialog.open(MensajeModalComponent, {
          data: {
            name: this.Lang === 'es'
              ? 'Contraseña actualizada correctamente. Por favor inicie sesión de nuevo.'
              : 'Password updated successfully. Please log in again.',
            title: '',
            isError: false,
          },
          panelClass: 'modal-overlay',
        });
        dialogRef.closed.subscribe(() => {
          this.authService.setMustUpdatePassword(false);
          this.authService.logout();
        });
      },
      error: () => {
        this.isLoading = false;
        this.dialog.open(MensajeModalComponent, {
          data: {
            name: this.Lang === 'es'
              ? 'Error al actualizar la contraseña. Intente nuevamente.'
              : 'Error updating password. Please try again.',
            title: 'Error',
            isError: true,
          },
          panelClass: 'modal-overlay',
        });
      },
    });
  }
}
