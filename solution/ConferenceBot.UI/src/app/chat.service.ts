import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, WritableSignal } from '@angular/core';
import { Observable, ReplaySubject, firstValueFrom, map, mergeMap, scan, tap } from 'rxjs';
import { BASE_URL } from './app.config';

type SessionCreationResponse = {
  session_id: string;
}

export enum Sender {
  User = 'user',
  Assistant = 'assistant'
}

export type Message = {
  sender: Sender;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  constructor(private client: HttpClient, @Inject(BASE_URL) private baseUrl: string) { }

  async createChatSession(): Promise<string> {
    const response = await firstValueFrom(this.client.post<SessionCreationResponse>(`${this.baseUrl}/chat/complete`, null));
    return response.session_id;
  }

  async getMessageHistory(sessionId: string): Promise<Message[]> {
    const response = await firstValueFrom(this.client.get<Message[]>(`${this.baseUrl}/chat/complete/${sessionId}/messages`));
    return response;
  }

  addAndRunBasic(sessionId: string, message: string): Observable<string> {
    return this.client.post(`${this.baseUrl}/chat/complete/${sessionId}/messages`, { message })
      .pipe(mergeMap(() => this.client.get<string>(`${this.baseUrl}/chat/complete/${sessionId}/run-basic`)));
  }

  addAndRun(sessionId: string, message: string): Observable<string> {
    return this.client.post(`${this.baseUrl}/chat/complete/${sessionId}/messages`, { message })
      .pipe(mergeMap(() => {
        return new Observable<string>(observer => {
          const eventSource = new EventSource(`${this.baseUrl}/chat/complete/${sessionId}/run`);
          eventSource.onmessage = event => observer.next(event.data)
        });
      }));
  }
}
