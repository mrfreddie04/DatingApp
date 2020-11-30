import { OnInit, TemplateRef, ViewContainerRef, Input, Directive } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]'  //*appHasRole='["Admin","Moderator"]"'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  user: User;

  //ViewContainerRef - represents a container where one or more views can be attached to a component
  //TemplateRef - represents an embeded template that can be used to instantiated emmbedded views
  constructor(private viewContainerRef: ViewContainerRef, 
      private templateRef: TemplateRef<any>,
      private accountService: AccountService) 
  { 
    this.accountService.currentUser$
      .pipe(take(1))
      .subscribe(user=>{
        this.user = user;        
      });
  }

  ngOnInit(): void {
    //clear the view if no roles
    if(this.user==null || !this.user?.roles)
    {
      this.viewContainerRef.clear();
      return;
    }
    
    //determines if the specified callback function retuens true for any element of an array
    if(this.user?.roles.some(r => this.appHasRole.includes(r)))
    {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    }
    else
    {
      this.viewContainerRef.clear();
    }
  }

  
}
