import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css']
})
export class DateInputComponent implements ControlValueAccessor {
  @Input() label: string;
  @Input() maxDate: Date;

  public bsConfig: Partial<BsDatepickerConfig>;
  
  constructor(@Self() public ngControl: NgControl) { 
    this.ngControl.valueAccessor = this;
    this.bsConfig = {
      containerClass: "theme-red", //class athat will be applied to dp container
      dateInputFormat: "DD MMMM YYYY" //12 October 1980
    };
  }

  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }

}
