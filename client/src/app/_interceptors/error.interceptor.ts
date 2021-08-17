import { ToastrService } from 'ngx-toastr';
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError( (err) => {
        if(err && err instanceof HttpErrorResponse) {
          switch(err.status) {
            case 400:
              if(err.error.errors) {
                const modalStateErrors = [];
                for(const key in err.error.errors) {
                  if(err.error.errors[key])
                    modalStateErrors.push(err.error.errors[key]);
                }
                //throws to the nearest try/catch block - to display in the component catching this error
                //throw modalStateErrors.flat(); 
                return throwError(modalStateErrors.flat());
              } else if(typeof(err.error)==="object") {
                //simple toastr message
                this.toastr.error(err.statusText === "OK" ? "Bad Request" : err.statusText, err.status.toString());
              } else {
                this.toastr.error(err.error, err.status.toString());
              }
              break;
            case 401:
              //simple toastr message
              this.toastr.error(err.statusText === "OK" ? "Uauthorized" : err.statusText, err.status.toString());
              break;
            case 404:
              //redirect to not-found page
              this.router.navigateByUrl("/not-found");
              break;
            case 500:
              const navigationExtras: NavigationExtras = {
                state: {
                  error: err.error
                }
              };
              //redirect to server-error page and pass some additional info (error object formatted by API)
              this.router.navigateByUrl("/server-error", navigationExtras);
              break;
            default:
              this.toastr.error("Something unexpected went wrong");
              console.log(err); //to further investigate and possibly tweak error handler.
              break;
          }
        }
        //console.log("Rethrowing the error");
        return throwError(err);
      })
    )
  }
}
