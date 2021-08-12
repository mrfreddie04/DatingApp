import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.css']
})
export class NotFoundComponent implements OnInit {
  currentUser$: Observable<User>;

  constructor(private accountService: AccountService) { 
    this.currentUser$ =  this.accountService.currentUser$;
  }

  ngOnInit(): void {
  }

}
