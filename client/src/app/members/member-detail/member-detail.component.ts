import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';

import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild("memberTabs", {static:true}) memberTabs: TabsetComponent; 
  activeTab: TabDirective;
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = []; //moved over from member-messages component

  constructor(private memberService: MembersService, private route: ActivatedRoute,
      private messageService: MessageService) { }

  ngOnInit(): void {
    //this.loadMember();
    this.route.data.subscribe(data=>{
      this.member = data.member;
      this.galleryImages = this.getImages();      
    });

    this.route.queryParams.subscribe(params=>{
      //check if we have sth in params.tab
      //if yes - navigate to this tab, otherwise select the first tab
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [{
        width:"500px",
        height:"500px",
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
     }];      
  }

  getImages() : NgxGalleryImage[]{
    const imageUrls : NgxGalleryImage[] = [];

    this.member.photos.forEach(image=>{
      imageUrls.push({
                        small: image?.url,
                        medium: image?.url,
                        big: image?.url
                      });
    });

    return imageUrls;
  }

  // loadMember(){
  //   const username: string = this.route.snapshot.paramMap.get("username");    
  //   this.memberService.getMember(username)
  //     .subscribe(member=>{
  //       this.member = member;
  //       this.galleryImages = this.getImages();
  //     });
  // }  

  loadMessages()
  {
    this.messageService.getMessageThread(this.member.username)
      .subscribe(result=>{
        this.messages = result;
      });
  }  

  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading=="Messages"  && this.messages.length==0){
      this.loadMessages();
    }
  }

  selectTab(tabId: number){
    this.memberTabs.tabs[tabId].active = true;
  }
}
