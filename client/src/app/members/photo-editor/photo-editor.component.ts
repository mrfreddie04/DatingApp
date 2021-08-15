import { Photo } from './../../_models/photo';
import { MembersService } from './../../_services/members.service';
import { AccountService } from 'src/app/_services/account.service';
import { environment } from './../../../environments/environment';
import { Component, Input, OnInit } from '@angular/core';
import { FileSelectDirective, FileDropDirective, FileUploader, FileItem } from 'ng2-file-upload';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';


@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member;

  uploader:FileUploader;
  hasBaseDropZoneOver:boolean;
  baseUrl:string = environment.apiUrl;
  user: User;

  constructor(private accountService: AccountService, private membersService: MembersService) { 
    this.accountService.currentUser$
      .pipe(take(1))
      .subscribe((user)=>this.user=user);
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}users/add-photo`,
      authToken: `Bearer ${this.user.token}`,
      isHTML5: true,
      allowedFileType: ["image"],
      removeAfterUpload: true, //remove from the drop zone after upload
      autoUpload: false, //we will trigger upload by a button
      maxFileSize: 10 * 1024 *1024
    });

    //set events
    this.uploader.onAfterAddingFile = (file: FileItem) => {
      file.withCredentials = false; //we use the bearer token
    };

    this.uploader.onSuccessItem = (fileItem, response, status, headers) => {
      if(response) {
        const photo: Photo = JSON.parse(response);
        this.member.photos.push(photo);
        if(photo.isMain) {
          this.updatePhoto(photo);
        }
      }
    };
  }

  public fileOverBase(e:any):void {
    this.hasBaseDropZoneOver = e;
  }

  public setMainPhoto(photo: Photo) {
    this.membersService.setMainPhoto(photo.id)
      .subscribe(() => {
        this.updatePhoto(photo);
      });
  }
 
  public deletePhoto(photo: Photo) {
    this.membersService.deletePhoto(photo.id).subscribe(
      () => {
        this.member.photos = this.member.photos.filter(p => p.id !== photo.id);
      }
    )
  }

  private updatePhoto(photo: Photo) {
    this.user.photoUrl = photo.url;
    this.accountService.setCurrentUser(this.user);
    this.member.photoUrl = photo.url;
    this.member.photos.forEach( p => {
      if(p.id === photo.id) { 
        p.isMain = true; 
      } else {
        p.isMain = false; 
      }
    });
  }
}
