import { Component, OnInit } from '@angular/core';

import { AccountService } from '../_services/account.service';
//import { User } from '../_models/user';
//import { Observable } from 'rxjs';
//import { Observable } from 'rxjs';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any={}; //to store form data
  //loggedIn: boolean;
  //currentUser$: Observable<User>;

  constructor(public accountService: AccountService) { };

  ngOnInit(): void {
    //this.getCurrentUser()
    //this.currentUser$ = this.accountService.currentUser$;
  }

  //method
  login(){
    this.accountService.login(this.model)
      .subscribe(res=>{
        console.log(res);
      },err=>{
        console.log(err);
      });
  };        

  logout(){
    this.accountService.logout();
    //this.loggedIn = false;    
  };

  // getCurrentUser(){
  //   this.accountService.currentUser$.subscribe(user=>{
  //     //this.loggedIn = !!user; //!! turrn object into boolean null=>false,
  //   }, error => {
  //     console.log(error);
  //   });
  // }
}
