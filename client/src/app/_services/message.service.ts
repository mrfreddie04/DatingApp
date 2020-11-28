import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container: string){

    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append("Container",container);

    return getPaginatedResult<Message[]>(this.baseUrl+"messages", params, this.http);
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(this.baseUrl+"messages/thread/"+username);
  }

  sendMessage(username: string, content: string){
    
    const message = {
      recipientUsername: username,
      content: content      
    };

    return this.http.post<Message>(this.baseUrl+"messages",message);
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl+"messages/"+id);  
  }
}
