import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef: BsModalRef;

  constructor(private modalService: BsModalService) { }

  confirm(title="Confirmation", 
    message="Are you sure you want to do this?", btnOKText="Ok", btnCancelText="Cancel"): Observable<boolean>
  {
    const config = {
      //class: "modal-dialog-centered",
      initialState: {
        title: title,
        message: message,
        btnOkText: btnOKText,
        btnCancelText: btnCancelText
      }
    };  

    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);

    return new Observable<boolean>(this.getResult());
  }

  private getResult() {
    //when caller subscribes to return value of confirm() method
    return (observer) => 
    {
      const subscription = this.bsModalRef.onHidden.subscribe(() => {
          observer.next(this.bsModalRef.content.result);
          observer.complete();
      });

      return {
        unsubscribe() {
            subscription.unsubscribe();
          }
      }  
    }
  }
}
