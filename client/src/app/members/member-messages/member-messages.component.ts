import { MessageService } from './../../_services/message.service';
import { Message } from './../../_models/message';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild("messageForm") messageForm: NgForm;
  //@Input() messages: Message[] = [];
  @Input() username: string;
  public messages: Message[] = [];
  public messageContent: string;

  constructor(public messageService: MessageService) { 
    this.messageService.messageThread$.subscribe( (messages) => {
      this.messages = messages;
    })    
  }

  ngOnInit(): void {
    //this.loadMessageThread();
  }

  public sendMessage(){
    this.messageService.sendMessage(this.username, this.messageContent)
      .then(()=>{
        console.log("SendMessage completed");
        this.messageForm.reset();
      })
    
    //   .subscribe((message)=>{
    //   this.messages.push(message);
    //   this.messageForm.reset();
    // });
  }

  // loadMessageThread() {
  //   this.messageService.getMessageThread(this.username).subscribe((response)=>{
  //     this.messages = response;
  //   });
  // }

}
