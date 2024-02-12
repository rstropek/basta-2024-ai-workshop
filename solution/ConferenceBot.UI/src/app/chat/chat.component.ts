import { Component, NgZone, OnInit, SecurityContext, signal } from '@angular/core';
import { ChatService, Message, Sender } from '../chat.service';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MarkdownModule, MarkdownService, SECURITY_CONTEXT } from 'ngx-markdown';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MarkdownModule],
  providers: [MarkdownService, { provide: SECURITY_CONTEXT, useValue: SecurityContext.HTML }],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit {
  private sessionId?: string;

  public messageHistory = signal<Message[]>([]);
  public userMessage = new FormControl('Give me 3 suggestions for topics for a database dev');

  constructor(private chat: ChatService, private ngZone: NgZone) { }

  async ngOnInit() {
    this.sessionId = await this.chat.createChatSession();
    const history = await this.chat.getMessageHistory(this.sessionId);
    this.messageHistory = signal(history);
  }

  async sendMessage() {
    if (!this.sessionId || !this.userMessage.value) {
      return;
    }

    const message = this.userMessage.value;
    this.userMessage.setValue('');

    this.messageHistory.update(h => [...h, { sender: Sender.User, message }]);
    this.chat.addAndRun(this.sessionId, message).subscribe(assistantMessage => {
    //this.chat.addAndRunBasic(this.sessionId, message).subscribe(assistantMessage => {
      assistantMessage = assistantMessage.replaceAll('\\n', '\n');

      let append = true;
      let lastMessage: Message | undefined;
      if (this.messageHistory().length !== 0) {
        lastMessage = this.messageHistory()[this.messageHistory().length - 1];
        if (lastMessage.sender === Sender.Assistant) {
          append = false;
        }
      }
      this.ngZone.run(() => {
        if (append) {
          this.messageHistory.update(h => [...h, { sender: Sender.Assistant, message: assistantMessage }]);
        } else {
          this.messageHistory.update(h => {
            h[this.messageHistory().length - 1].message += assistantMessage;
            return h;
          });
        }
      });
    });
  }
}
