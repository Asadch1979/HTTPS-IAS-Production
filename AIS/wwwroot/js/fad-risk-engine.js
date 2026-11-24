(function($){'use strict';$(function(){
 var root=$('#riskEngine'),catalog=[],busy=false;
 if(!root.length)return;
 var period=$('#enginePeriod'),type=$('#engineType'),entity=$('#engineEntity'),status=$('#engineStatus');
 function selection(){return {period:period.val(),type:type.val(),entity:entity.val()||0};}
 function options(select,rows,key,label,all){select.empty();if(all)select.append($('<option>').val(0).text('All entities'));var seen=new Set();rows.forEach(function(r){if(!seen.has(r[key])){seen.add(r[key]);select.append($('<option>').val(r[key]).text(r[label]));}});}
 function fail(xhr){status.text(xhr.responseJSON&&xhr.responseJSON.message||'Request failed. Contact the administrator with the request reference.');}
 period.on('change',function(){options(type,catalog.filter(function(r){return String(r.period_id)===period.val();}),'type_id','type_id');type.trigger('change');});
 type.on('change',function(){options(entity,catalog.filter(function(r){return String(r.period_id)===period.val()&&String(r.type_id)===type.val();}),'entity_id','name',true);});
 function details(id){var data=selection();data.result=id;$.getJSON(root.attr('data-results'),data).done(function(r){
  var box=$('#engineDetails').empty();r.data.exceptions.forEach(function(x){box.append($('<p>').text(x.code+': '+x.message));});
  var table=$('<table>').addClass('table'),head=$('<tr>');['Level','Source code','Observation','Instances','Status'].forEach(function(x){head.append($('<th>').text(x));});table.append($('<thead>').append(head));var body=$('<tbody>');
  r.data.detail.forEach(function(d){var tr=$('<tr>');[d.node_level,d.source_code||d.procedure_code||d.process_code||d.package_code||d.main_code,d.observation_id,d.instances,d.status].forEach(function(v){tr.append($('<td>').text(v==null?'—':v));});body.append(tr);});box.append(table.append(body));
 }).fail(fail);}
 function render(rows,target){var body=$(target).empty();rows.forEach(function(row){var tr=$('<tr>');
 var pending=row.status==='PENDING_INPUT', label=row.status==='NOT_EXECUTED'?'Not Executed':row.status.replace(/_/g,' ');
 [row.period_id,row.entity_name,row.eng_id,row.observations,pending?'Pending Input':row.score,pending?'Pending Input':row.risk_rating,label,row.calculated_on].forEach(function(v){tr.append($('<td>').text(v==null?'—':v));});
 var cell=$('<td>');if(row.result_id){cell.append($('<button>').attr('type','button').addClass('btn btn-link').text('Run '+row.run_id+' / requirements').on('click',function(){details(row.result_id);}));
 if(row.processed!=null)cell.append($('<div>').text(row.processed+' processed; '+row.calculated+' calculated; '+row.pending+' pending; '+row.skipped+' skipped; '+row.failed+' failed.'));}
 else cell.text('Execution required');tr.append(cell);body.append(tr);});}
 var requestNumber=0;
 function load(){if(!period.val())return;var number=++requestNumber;$.getJSON(root.attr('data-results'),selection()).done(function(r){if(number!==requestNumber)return;
 if(!r||!r.success){status.text('Results could not be loaded.');return;}
 render(r.data.current,'#engineCurrentRows');render(r.data.cursor,'#engineRows');}).fail(fail);}
 $('#engineLoad').on('click',load);
 function run(bulk){if(busy)return;var data=selection();if(!data.period||!data.type||(!bulk&&Number(data.entity)<=0)){status.text('Select the required filters.');return;}if(!window.confirm(bulk?'Execute all engagements for this period and entity type in your authorized office?':'Recalculate every engagement for this entity and period as a new historical run?'))return;
 data.__RequestVerificationToken=root.find('input[name="__RequestVerificationToken"]').val();busy=true;root.find('button,select').prop('disabled',true);status.text('Executing…');
 $.post(root.attr(bulk?'data-bulk':'data-single'),data).done(function(r){var x=r.data;status.text('Run '+x.runId+': '+x.processed+' processed, '+x.calculated+' calculated, '+x.skipped+' skipped, '+x.pending+' pending, '+x.failed+' failed.');load();}).fail(fail).always(function(){busy=false;root.find('button,select').prop('disabled',false);});
 }
 $('#engineBulk').on('click',function(){run(true);});$('#engineSingle').on('click',function(){run(false);});
 $.getJSON(root.attr('data-catalog')).done(function(r){catalog=r.data.cursor;options(period,catalog,'period_id','period_id');if(catalog.some(function(r){return Number(r.period_id)===5;}))period.val('5');period.trigger('change');load();}).fail(fail);
});})(jQuery);
