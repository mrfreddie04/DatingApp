import { element } from 'protractor';
import { Message } from './../../_models/message';
import { MessageService } from './../../_services/message.service';
import { MembersService } from './../../_services/members.service';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Photo } from 'src/app/_models/photo';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';


@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild("memberTabs", {static: true}) memberTabs: TabsetComponent;
  public activeTab: TabDirective;
  public member: Member;
  public messages: Message[] = [];
  public galleryOptions: NgxGalleryOptions[];
  public galleryImages: NgxGalleryImage[]=[];

  constructor(
    private memberService: MembersService, 
    private route: ActivatedRoute,
    private messageService: MessageService) { 
  }

  ngOnInit(): void {
    //this.loadMember();
    this.route.data.subscribe((data)=>{
      this.member = data.member;
    });

    this.route.queryParams.subscribe((params: Params) => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        thumbnailsColumns: 4,
        imagePercent: 100,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];   

    this.galleryImages = this.getImages();
  }

  private getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    this.member.photos.forEach((photo: Photo)=>{
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      });
    });    
    return imageUrls;
  }

  // public loadMember() {
  //   const username = this.route.snapshot.paramMap.get("username");
  //   this.memberService.getMember(username).subscribe((member)=>{
  //     this.member = member;
  //     this.galleryImages = this.getImages();
  //   });
  // }

  public onTabActivated(data: TabDirective) {
    this.activeTab = data;
    console.log(this.activeTab.heading)
    if(this.activeTab.heading === "Messages" && this.messages.length === 0) {
      this.loadMessageThread();
    }
  }

  public selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  private loadMessageThread() {
    console.log("Loading Message Thread...");
    this.messageService.getMessageThread(this.member.username).subscribe((response)=>{
      this.messages = response;
    });
  }  
}
