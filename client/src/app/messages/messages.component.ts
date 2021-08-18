import { MessageService } from './../_services/message.service';
import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  public messages: Message[] = [];
  public pagination: Pagination;
  public container: string = "Unread";
  public pageSize: number = 5;
  public pageNumber: number = 1; 
  public loading: boolean = false;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  public pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }

  public getMemberRoute(message: Message) {
    const memberName = this.container === "Outbox" ? message.recipientUsername: message.senderUsername;
    return `/members/${memberName}`;
  }

  public loadMessages() {
    this.loading = true;
    this.messageService.getMessagesForUser(this.container, this.pageNumber, this.pageSize)
      .subscribe( (paginatedResult) => {
        this.messages = paginatedResult.result;
        this.pagination = paginatedResult.pagination;        
        this.loading = false;
      });
  }

  public deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe(()=>{
      const messageIdx = this.messages.findIndex(m => m.id === id);
      if(messageIdx>0)
        this.messages.splice(messageIdx,1);
    })
  }
}
