import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Injectable } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { BehaviorSubject, ReplaySubject } from 'rxjs';
import { take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl: string = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  public onlineUsers$  = this.onlineUsersSource.asObservable();  

  constructor(
    private toastr: ToastrService,
    private router: Router
  ) { }

  public createHubConnection(user: User) {
    //create connection
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    //start connection
    this.hubConnection
      .start()
      .catch( error => console.error(error));

    //listed to events
    this.hubConnection.on("UserIsOnline",(username:string)=>{
      //this.toastr.info(username+ " has connected");
      this.onlineUsers$ .pipe(take(1)).subscribe((usernames: string[]) => {
        this.onlineUsersSource.next([...usernames,username]);
      });  
    });

    this.hubConnection.on("UserIsOffline",(username:string)=>{
      this.onlineUsers$ .pipe(take(1)).subscribe((usernames: string[]) => {
        this.onlineUsersSource.next([...usernames.filter(user => user !== username)]);
      });  
    });    

    this.hubConnection.on("GetOnlineUsers", (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    })

    this.hubConnection.on("NewMessageReceived", ({username, knownAs}: {username: string; knownAs: string;}) => {
      this.toastr.info(`${knownAs} has sent you a new message!`)
        .onTap //toastclick
        .pipe(take(1))
        .subscribe( () => this.router.navigateByUrl(`/members/${username}?tab=3`))
    })    
  }

  public stopHubConnection() {
    if(this.hubConnection) { // && this.hubConnection.state !==  HubConnectionState.Disconnected) {
      this.hubConnection  
      .stop()
      .catch( error => console.error(error));
    }
  }
}
