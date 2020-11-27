import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member;
  uploader: FileUploader;
  hasBaseDropZoneOver: boolean = false; //indicate if the file is over drop zone
  baseUrl: string = environment.apiUrl;
  user: User;
  
  constructor(private accountService: AccountService,
      private memberService: MembersService,
      private toastr: ToastrService) 
  { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user=user;
    });
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any){
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl+"users/add-photo", //end point we will send the upload to
      authToken: `Bearer ${this.user.token}`, //does not go through the interceptor
      isHTML5: true,
      allowedFileType: ["image"],
      removeAfterUpload: true, //remove from the drop zone after upload has taken place
      autoUpload: false,
      maxFileSize: 10*1024*1024
    });

    this.uploader.onAfterAddingFile = (file)=>{
      //otherwise we will have to make an adjustment to api calls configuration and allow cretentials
      //to go up with our request
      file.withCredentials = false; 
    };

    //after successful upload of each file
    this.uploader.onSuccessItem = (item,response,status,headers)=>{
      if(response) {
        const photo: Photo = JSON.parse(response); //photo passedback from API
        this.member.photos.push(photo);
        if(photo.isMain)
        {
          this.user.photoUrl = photo.url;
          this.member.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
        }
      }  
    };
  }

  setMainPhoto(photo: Photo){
    //save member
    console.log("Photo",photo)
    this.memberService.setMainPhoto(photo.id)
      .subscribe(()=>{
        this.user.photoUrl = photo.url;
        this.accountService.setCurrentUser(this.user);
        this.member.photoUrl = photo.url;

        this.member.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        })

        // const oldMain = this.member.photos.find(p=>p.isMain);
        // if(oldMain && oldMain != undefined) oldMain.isMain = false;
        // const newMain = this.member.photos.find(p=>p.id == photo.id);
        // if(newMain && newMain != undefined) newMain.isMain = true;
        //this.toastr.success("Main photo updated successfully");
      })
  }  

  deletePhoto(photoId: number){
    this.memberService.deletePhoto(photoId)
      .subscribe(()=>{
        // const photoIdx = this.member.photos.findIndex(p=>p.id == photoId);
        // if(photoIdx>=0) this.member.photos.splice(photoIdx,1);
        this.member.photos = this.member.photos.filter(photo=>photo.id!=photoId);
      });
  }

}
