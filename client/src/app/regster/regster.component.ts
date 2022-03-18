import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-regster',
  templateUrl: './regster.component.html',
  styleUrls: ['./regster.component.css']
})
export class RegsterComponent implements OnInit {
   @Output() cancelRegister = new EventEmitter();
  model: any ={};

  constructor(private accountService: AccountService, private toastr:ToastrService) { }

  ngOnInit(): void {
  }

  register(){
    this.accountService.register(this.model).subscribe(response=>{
      console.log(response);
      this.cancel();
    }, error =>{
      console.log(error);
      this.toastr.error(error.error)
    })

    
  }
  cancel(){
    this.cancelRegister.emit(false);
  }

}
