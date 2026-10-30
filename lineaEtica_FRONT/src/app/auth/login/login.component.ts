import { Component, OnInit, OnDestroy, inject, afterNextRender, NgZone } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { Dialog } from '@angular/cdk/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../auth.service';
import { MensajeModalComponent } from '../mensaje-modal/mensaje-modal.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslateModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(Dialog);
  private translate = inject(TranslateService);
  private ngZone = inject(NgZone);

  isLoading = false;
  Lang = 'es';
  isDarkMode = false;
  private captchaCode = '';

  carouselImages = [
    'assets/images/La Línea Ética, una obligación inaplazable.jpg',
    'assets/images/2-1-1024x576.jpg',
    'assets/images/LPS-Blog-Fraude.jpg',
  ];
  activeSlide = 0;
  private carouselInterval: ReturnType<typeof setInterval> | null = null;

  form: FormGroup = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
    captcha: new FormControl('', [Validators.required]),
  });

  constructor() {
    afterNextRender(() => {
      this.generateCaptcha();
    });
  }

  ngOnInit(): void {
    this.Lang = localStorage.getItem('language') || 'es';
    this.translate.use(this.Lang);
    this.isDarkMode = localStorage.getItem('theme') === 'dark';
    this.carouselInterval = setInterval(() => {
      this.activeSlide = (this.activeSlide + 1) % this.carouselImages.length;
    }, 4500);
  }

  ngOnDestroy(): void {
    if (this.carouselInterval) clearInterval(this.carouselInterval);
  }

  nextSlide(): void {
    this.activeSlide = (this.activeSlide + 1) % this.carouselImages.length;
  }

  prevSlide(): void {
    this.activeSlide = (this.activeSlide - 1 + this.carouselImages.length) % this.carouselImages.length;
  }

  goToSlide(index: number): void {
    this.activeSlide = index;
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
  }

  changeLanguage(lang: string): void {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
    this.Lang = lang;
  }

  login(): void {
    if (this.form.controls['captcha'].value !== this.captchaCode) {
      this.openModal(
        this.Lang === 'es'
          ? 'El CAPTCHA ingresado es incorrecto. Por favor, inténtelo de nuevo.'
          : 'The CAPTCHA you entered is incorrect. Please try again.',
        '',
        true
      );
      this.generateCaptcha();
      return;
    }

    this.isLoading = true;
    const { username, password } = this.form.value;

    this.authService.login(username, password).subscribe({
      next: (response: any) => {
        this.authService.setToken(response);
        this.isLoading = false;
        const mustUpdate = !!response?.actualizarPass;
        this.authService.setMustUpdatePassword(mustUpdate);
        if (mustUpdate) {
          this.router.navigate(['/auth/cambiar-password']);
        } else {
          this.router.navigate(['/pages/inicio']);
        }
      },
      error: () => {
        this.isLoading = false;
        this.openModal(
          this.Lang === 'es'
            ? 'Error al iniciar sesión. Por favor valide credenciales'
            : 'Error logging in. Please validate credentials',
          'Error',
          true
        );
      },
    });
  }

  refreshCaptcha(): void {
    this.generateCaptcha();
  }

  private openModal(name: string, title: string, isError: boolean): void {
    this.dialog.open(MensajeModalComponent, {
      data: { name, title, isError },
      panelClass: 'modal-overlay',
    });
  }

  private generateCaptcha(): void {
    this.captchaCode = this.createCaptcha();
    this.ngZone.runOutsideAngular(() => {
      setTimeout(() => this.renderCaptcha(), 50);
    });
  }

  private createCaptcha(): string {
    const chars = 'ABCDEFGHJKMNOPQRSTUVWXYZabcdefghjklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < 6; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  }

  private renderCaptcha(): void {
    const canvas = document.getElementById('captchaCanvas') as HTMLCanvasElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.font = '30px Arial';
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'center';
    for (let i = 0; i < 5; i++) {
      ctx.strokeStyle = this.randomColor();
      ctx.beginPath();
      ctx.moveTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.lineTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.stroke();
    }
    ctx.fillStyle = this.randomColor();
    ctx.fillText(this.captchaCode, canvas.width / 2, canvas.height / 2);
    for (let i = 0; i < 150; i++) {
      ctx.fillStyle = this.randomColor();
      ctx.fillRect(Math.random() * canvas.width, Math.random() * canvas.height, 1, 1);
    }
  }

  private randomColor(): string {
    const h = '0123456789ABCDEF';
    return '#' + Array.from({ length: 6 }, () => h[Math.floor(Math.random() * 16)]).join('');
  }
}
