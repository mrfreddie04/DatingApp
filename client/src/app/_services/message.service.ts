import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from './../../environments/environment';
import { PaginationHelper } from './paginationHelper';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) { }

  public createMessage() {

  }

  public getMessagesForUser(container: string, pageNumber: number, pageSize: number) {

    let params = PaginationHelper.getPaginationHeaders(pageNumber,pageSize);
    params = params.append("container",container);

    return PaginationHelper.getPaginatedResults<Message[]>(`${this.baseUrl}messages` ,params, this.http);
  }

  public getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`);
  }

  public sendMessage(username: string, content: string) {
    const createMessage = {
      recipientUsername: username,
      content: content
    };
    return this.http.post<Message>(`${this.baseUrl}messages`, createMessage);
  }

  public deleteMessage(messageid: number) {
    return this.http.delete(`${this.baseUrl}messages/${messageid}`, {});
  }
}
