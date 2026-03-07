(function(){var h=document.getElementById('fieldAuditVoucherReplica');if(!h)return;var engId=h.dataset.engId;
fetch((window.g_asiBaseURL||'')+'/ApiCalls/Get_Working_Paper_Voucher_Checking',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded; charset=UTF-8'},body:'ENGID='+encodeURIComponent(engId),credentials:'same-origin'})
.then(r=>r.json()).then(function(data){var b=document.querySelector('#fieldAuditVoucherTable tbody');b.innerHTML='';(data||[]).forEach((v,i)=>b.insertAdjacentHTML('beforeend','<tr><td>'+(i+1)+'</td><td>'+(v.v_NUMBER||'')+'</td><td>'+(v.observation||'')+'</td><td>'+(v.parA_NO||'')+'</td></tr>'));if(typeof initializeDataTable==='function'){initializeDataTable('fieldAuditVoucherTable');}});
})();
