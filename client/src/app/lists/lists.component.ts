import { PaginatedResult, Pagination } from './../_models/pagination';
import { MembersService } from './../_services/members.service';
import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { Like } from '../_models/like';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;
  pagination: Pagination;
  predicate: string = "liked";
  pageNumber: number = 1;
  pageSize: number = 5;

  constructor(private membersService: MembersService) { }

  ngOnInit(): void {
    this.loadLikes();
  }

  public pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadLikes();
  }  

  loadLikes() {
    this.membersService.getLikes(this.predicate, this.pageNumber, this.pageSize)
      .subscribe((paginatedResult: PaginatedResult<Like[]>)=>{
      this.members = paginatedResult.result as Partial<Member[]>;
      this.pagination = paginatedResult.pagination;
    })
  }

}
