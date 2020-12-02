import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl: string = environment.hubUrl;
  private hubConnection: HubConnection;

  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router ) { }

  //Create the hub connection
  //when a user connects to our applicayion and they are authenticated 
  //we will automatically create a hub connection that is going to connect him to our PresenceHub
  createHubConnection(user:User)
  {
    //create hub connection
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "presence",{
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    //start hub connection
    this.hubConnection
      .start()
      .catch(error=>console.log(error));

    //listen for the server events
    this.hubConnection.on("UserIsOnline", username=>{
      this.onlineUsers$.pipe(take(1))
        .subscribe(usernames=>{
        //we do not want tomutate usernames? may interfere with angular  
        this.onlineUsersSource.next([...usernames,username])
        });
      });
      //this.toastr.info(username+" has connected"); 

    this.hubConnection.on("UserIsOffline", username=>{
      this.onlineUsers$.pipe(take(1))
        .subscribe(usernames=>{
        //we do not want tomutate usernames? may interfere with angular  
        this.onlineUsersSource.next([...usernames.filter(elem=>elem!=username)])
        });      
      //this.toastr.warning(username+" has disconnected");
    });      

    this.hubConnection.on("GetOnlineUsers", (onlineUsers:string[])=>{
      //console.log(onlineUsers);
      this.onlineUsersSource.next(onlineUsers);  
    });    

    this.hubConnection.on("NewMessageReceived", ({username,knownAs}) => {
      this.toastr.info(knownAs + " has has sent you a new message!")
        .onTap //if the recipient clicks on the message
        .pipe(take(1))
        .subscribe(()=>{
          this.router.navigateByUrl("/members/"+username+"?tab=3");
        })
    });
  }

  stopHubConnection()
  {
    this.hubConnection
      .stop()
      .catch(error=>console.log(error));
  }

}
