import { BusyService } from './busy.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from './../../environments/environment';
import { PaginationHelper } from './paginationHelper';
import { Message } from '../_models/message';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl: string = environment.apiUrl;
  hubUrl: string = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  public messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient, private busyService: BusyService) { }

  public createHubConnection(user: User, otherUsername: string) {
    //console.log("CHB",user,otherUsername);
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(`${this.hubUrl}message?user=${otherUsername}`, {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .catch( error => console.error(error))
      .finally(() => this.busyService.idle());

    this.hubConnection.on("ReceiveMessageThread",(messages: Message[])=>{
      this.messageThreadSource.next(messages);
    })

    this.hubConnection.on("NewMessage",(message: Message)=>{
      this.messageThread$.pipe(take(1)).subscribe((messages:Message[]) => {
        this.messageThreadSource.next([...messages,message]);
      });      
    })    

    this.hubConnection.on("UpdatedGroup", (group: Group)=>{
      if(group.connections.some( c => c.userName === otherUsername)) {
        //the other user is inthe chat box - mark all OUR messages as read
        this.messageThread$.pipe(take(1)).subscribe((messages:Message[]) => {
          const updatedMessages = messages.map( m => {
            if(!m.dateRead) m.dateRead  = new Date(Date.now());
            return m;  
          })
          this.messageThreadSource.next([...updatedMessages]);
        });          
      }
    });
  }

  public stopHubConnection() {
    if(this.hubConnection) {
      this.messageThreadSource.next([]);
      this.hubConnection  
      .stop()
      .catch( error => console.error(error));
    }
  }  

  public getMessagesForUser(container: string, pageNumber: number, pageSize: number) {

    let params = PaginationHelper.getPaginationHeaders(pageNumber,pageSize);
    params = params.append("container",container);

    return PaginationHelper.getPaginatedResults<Message[]>(`${this.baseUrl}messages` ,params, this.http);
  }

  // public getMessageThread(username: string) {
  //   return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`);
  // }

  public async sendMessage(username: string, content: string) {
    const createMessage = {
      recipientUsername: username,
      content: content
    };
    return this.hubConnection.invoke<void>("SendMessage", createMessage)
      .catch(err => console.error(err));
    //return this.http.post<Message>(`${this.baseUrl}messages`, createMessage);
  }

  public deleteMessage(messageid: number) {
    return this.http.delete(`${this.baseUrl}messages/${messageid}`, {});
  }
}
