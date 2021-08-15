import { environment } from './../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  public baseUrl: string = environment.apiUrl;
  members: Member[] = [];

  constructor(private http: HttpClient) { }

  public getMembers() {
    if(this.members.length>0) {
      return of(this.members);
    }
    // const httpOptions = this.getHttpOptions();
    return this.http.get<Member[]>(`${this.baseUrl}users`).pipe(
      tap((members) => {
        this.members = members;
      })
    )
  }

  public getMember(username: string) {
    // const httpOptions = this.getHttpOptions(); 
    const member = this.members.find( m => m.username === username);
    if(member) {
      return of(member);
    }
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);//, httpOptions);
  }

  public updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users`, member).pipe(
      tap(() => {
        const index = this.members.findIndex(item=>item.username===member.username);
        if(index>=0)
          this.members[index] = member;
      })
    );
  }

  public setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {});
        // .pipe(
        // switchMap(() => {}),
        // tap(() => {
        //   this.
        // })
  }  

  public deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`, {});
  }

  // private getHttpOptions() {
  //   const user: User = JSON.parse(localStorage.getItem("user"));
  //   const httpOptions = {
  //     headers: new HttpHeaders({
  //       Authorization: `Bearer ${user?.token}`
  //     })
  //   };
  //   return httpOptions;
  // }
}
