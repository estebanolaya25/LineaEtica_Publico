import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private http = inject(HttpClient);
  private config: { apiUrl: string; checkNameApiUrl: string } = {
    apiUrl: '',
    checkNameApiUrl: '',
  };

  load(): Promise<void> {
    const bust = `?v=${Date.now()}`;
    return firstValueFrom(
      this.http.get<{ apiUrl: string; checkNameApiUrl: string }>(`/assets/config.json${bust}`)
    ).then((data) => {
      if (data) this.config = data;
    });
  }

  get apiUrl(): string {
    return this.config.apiUrl;
  }

  get checkNameApiUrl(): string {
    return this.config.checkNameApiUrl;
  }
}
