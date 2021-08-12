import { AccountService } from './../_services/account.service';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  //loggedIn: boolean = false;
  currentUser$: Observable<User>

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    this.currentUser$ =  this.accountService.currentUser$;
    //this.getCurrentUser(); 
  }

  login() {
    this.accountService.login(this.model).subscribe(
      (response)=>{
        console.log(response);
      },
      (err) => {
        console.error(err);
      }
    );
  }

  logout() {
    this.accountService.logout();
  }

  // getCurrentUser() {
  //   this.currentUser$ =  this.accountService.currentUser$;
  //   this.accountService.currentUser$.subscribe( 
  //     (user) => {
  //       this.loggedIn = !!user;
  //     },
  //     (err) => {
  //       console.error(err);
  //     }
  //   )
  // }
}
