import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  pagination: Pagination;
  members: Partial<Member[]>;
  predicate: string = "liked";
  pageSize: number = 5;
  pageNumber: number = 1;
  
  constructor(private memberService: MembersService) {
  }

  ngOnInit(): void {
    this.loadLikes();
  }

  pageChanged(event: any) {  
    this.pageNumber = event.page;
    this.loadLikes();
  }    
    
  loadLikes(){
    this.memberService.getLikes(this.predicate,this.pageNumber,this.pageSize)
      .subscribe(response => {
        this.members = response.result;
        this.pagination = response.pagination;
      });
  }

}
