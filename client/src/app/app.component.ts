import { AccountService } from './_services/account.service';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';

interface UserFull {
  id: number;
  userName: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'The Dating App';
  users: UserFull[] = [];

  constructor(
    private accountService: AccountService
  ){}

  ngOnInit(): void {
    this.setCurrentUser();
  }

  private setCurrentUser(){
    const user: User = JSON.parse(localStorage.getItem("user"));
    this.accountService.setCurrentUser(user);
  }

}
