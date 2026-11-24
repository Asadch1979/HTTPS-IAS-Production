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
 function load(){if(!period.val())return;$.getJSON(root.attr('data-results'),selection()).done(function(r){var body=$('#engineRows').empty();r.data.cursor.forEach(function(row){var tr=$('<tr>');[row.period_id,row.entity_name,row.eng_id,row.observations,row.score,row.risk_rating,row.status,row.calculated_on].forEach(function(v){tr.append($('<td>').text(v==null?'—':v));});tr.append($('<td>').append($('<button>').attr('type','button').addClass('btn btn-link').text('Run '+row.run_id).on('click',function(){details(row.result_id);})));body.append(tr);});}).fail(fail);}
 $('#engineLoad').on('click',load);
 function run(bulk){if(busy)return;var data=selection();if(!data.period||!data.type||(!bulk&&Number(data.entity)<=0)){status.text('Select the required filters.');return;}if(!window.confirm(bulk?'Execute all engagements for this period and entity type in your authorized office?':'Recalculate every engagement for this entity and period as a new historical run?'))return;
 data.__RequestVerificationToken=root.find('input[name="__RequestVerificationToken"]').val();busy=true;root.find('button').prop('disabled',true);status.text('Executing…');
 $.post(root.attr(bulk?'data-bulk':'data-single'),data).done(function(r){var x=r.data;status.text('Run '+x.runId+': '+x.processed+' processed, '+x.skipped+' skipped, '+x.pending+' pending, '+x.failed+' failed.');load();}).fail(fail).always(function(){busy=false;root.find('button').prop('disabled',false);});
 }
 $('#engineBulk').on('click',function(){run(true);});$('#engineSingle').on('click',function(){run(false);});
 $.getJSON(root.attr('data-catalog')).done(function(r){catalog=r.data.cursor;options(period,catalog,'period_id','period_id');period.trigger('change');}).fail(fail);
});})(jQuery);
