import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[] = [];
  pagination: Pagination;
  container: string = "Unread";
  pageSize: number = 5;
  pageNumber: number = 1;  
  loading: boolean = false;

  constructor(private messageService: MessageService, private confirmService: ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
      this.messageService.getMessages(this.pageNumber, this.pageSize, this.container)
        .subscribe(response => {          
          this.messages = response.result;
          this.pagination = response.pagination;
          this.loading = false;
        });
  }

  pageChanged(event: any)  {
    this.pageNumber = event.page;    
    this.loadMessages();
  }

  deleteMessage(id: number)
  {
    this.confirmService.confirm("Confirmd delete message", "This cannot be undone")
      .subscribe(result=>{
        if(result)
        {
          //OK to delete
          this.messageService.deleteMessage(id)
          .subscribe(()=>{
            //this.messages = this.messages.filter(item=>item.id != id);
            this.messages.splice(this.messages.findIndex(m=>m.id == id),1);
          });          
        }
      });
  }  

}
