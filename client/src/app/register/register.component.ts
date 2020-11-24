import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // @Input() usersFromHomeComponent: any;
  @Output() cancelRegister = new EventEmitter();

  registerForm: FormGroup;
  maxDate: Date;
  validationErrors: string[] = [];

  constructor(private service: AccountService, 
    private router: Router,
    private toastr: ToastrService,
    private fb: FormBuilder) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }

  initializeForm(): void {
    this.registerForm = this.fb.group({
      gender: ["male"], //initial value, validator      
      username: ["",Validators.required],
      knownAs: ["",Validators.required],
      dateOfBirth: ["",Validators.required],
      city: ["",Validators.required],
      country: ["",Validators.required],
      password: ["",[Validators.required,Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword: ["", [Validators.required, this.matchValues("password")]]
    });
  }

  matchValues(matchTo: string): ValidatorFn
  {
    return (control : AbstractControl) => {
      return control?.value == control?.parent?.controls[matchTo]?.value 
        ? null //passing
        : { isMatching: true } //failing, we provide custom validation error object
    };    
  }

  register(){
    //console.log(this.registerForm.value);
    this.service.register(this.registerForm.value).subscribe(res=>{
      this.router.navigateByUrl("/members");
    },err=>{
      this.validationErrors = err;
      //this.toastr.error(err.error);
    });
  }

  cancel(){
    this.cancelRegister.emit(false);
  }
}
