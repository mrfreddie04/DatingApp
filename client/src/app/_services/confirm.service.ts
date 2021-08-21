import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Observer } from 'rxjs';
import { ConfirmDialogComponent } from '../_modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef: BsModalRef;

  constructor(private modalService: BsModalService) { }  

  public confirm(
      title: string = "Confirmation", 
      message: string = "Are you sure you want to do this?",
      btnOkText: string = "OK",
      btnCancelText: string = "Cancel"
  ): Observable<boolean> {
    const config = {
      class: "modal-dialog-centered",
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);
    //return this.bsModalRef.content.confirmResult;
    return new Observable<boolean>(this.getResult());
  }

  private getResult() {
     return (observer: Observer<boolean>) => 
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
     };
  }
}
