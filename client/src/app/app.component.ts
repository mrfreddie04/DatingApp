import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface User {
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
  users: User[] = [];

  constructor(private http: HttpClient){}

  ngOnInit(): void {
    this.getUsers();
  }

  getUsers() {
    this.http.get<User[]>("https://localhost:5001/api/users")
    .subscribe(
      (response)=>{
        this.users = response;
      },
      (err) => {
        console.log("Error", err);
      }
    );     
  }

}
