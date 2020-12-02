import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl: string = environment.apiUrl;
  hubUrl: string = environment.hubUrl;

  private hubConnection: HubConnection;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "message?user=" + otherUsername,
      {
        accessTokenFactory: () => user.token
      })
    .withAutomaticReconnect()
    .build();

    this.hubConnection.start()
      .catch(error=>console.log(error));   
      
    this.hubConnection.on("ReceiveMessageThread", messages=>{
      this.messageThreadSource.next(messages);
    });  

    this.hubConnection.on("NewMessage", (message:Message) => {
      //const messages; 
      this.messageThread$.pipe(take(1))
        .subscribe(messages=>{
        this.messageThreadSource.next([...messages,message])
        });
      });

    this.hubConnection.on("UpdatedGroup", (group: Group) => {        
      if (group.connections.some(x => x.username === otherUsername)) {          
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {                
              message.dateRead = new Date(Date.now())
            }
          })
          this.messageThreadSource.next([...messages]);
        })
      }
    });
  }

  stopHubConnection() {
    if(this.hubConnection) {
      this.hubConnection.stop()
        .catch(error=>console.log(error));      
    }    
  }

  getMessages(pageNumber: number, pageSize: number, container: string){

    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append("Container",container);

    return getPaginatedResult<Message[]>(this.baseUrl+"messages", params, this.http);
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(this.baseUrl+"messages/thread/"+username);
  }

  //adding async guarantees we are returnihn the promise
  async sendMessage(username: string, content: string){
    
    const message = {
      recipientUsername: username,
      content: content      
    };

    return this.hubConnection.invoke("SendMessage", message)
              .catch(error => console.log(error));

    //return this.http.post<Message>(this.baseUrl+"messages",message);
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl+"messages/"+id);  
  }
}
