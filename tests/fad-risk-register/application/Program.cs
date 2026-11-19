using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AIS.Models;
int checks=0;
void Expect(object value,bool valid) {
 var errors=new List<ValidationResult>();
 if (Validator.TryValidateObject(value,new ValidationContext(value),errors,true)!=valid)
  throw new Exception("Unexpected validation result: "+string.Join(";",errors.Select(e=>e.ErrorMessage)));
 checks++;
}
Expect(new FadRiskDetailInput {Code="A-1",Instances=0},true);
Expect(new FadRiskDetailInput {Code="A-1",Instances=1},false);
Expect(new FadRiskDetailInput {Code="A-1",Instances=1.5m,Observation="1",Evidence="2"},false);
Expect(new FadRiskDetailInput {Code="A-1",Instances=1,Observation="1",Evidence="2"},true);
Expect(new FadRiskDetailInput {Code="RM33",Base=0,Actual=20,Evidence="WP:123"},false);
Expect(new FadRiskDetailInput {Code="RM33",Base=100,Actual=20,Evidence="WP:123"},true);
Expect(new FadRiskDetailInput {Code="RM64",Actual=-22.69m,Evidence="WP:123"},true);
Expect(new FadRiskDetailInput {Code="RM64",Actual=1,Evidence="https://evil.test"},false);
Expect(new FadRiskDetailInput {Code="A-1",Instances=1,Evidence="WP:123",Observation="4"},false);
Expect(new FadRiskTransitionRequest {EngagementId=1,AssessmentId=1,Revision=1,Action="APPROVE"},false);
Expect(new FadRiskTransitionRequest {EngagementId=1,AssessmentId=1,Revision=1,Action="APPROVE",Remarks="Reviewed"},true);
Expect(new FadRiskTransitionRequest {EngagementId=1,AssessmentId=1,Revision=1,Action="DELETE",Remarks="x"},false);
var parsed=JsonSerializer.Deserialize<FadRiskAssessmentRequest>("""{"EngagementId":1,"RoleId":999,"EntityId":999,"SessionId":"forged","Details":[]} """);
if (typeof(FadRiskAssessmentRequest).GetProperty("RoleId")!=null || parsed.EngagementId!=1) throw new Exception("Authority leaked into request model");
var payload=JsonSerializer.Serialize(new FadRiskDetailInput {Code="RM33",Base=100,Actual=20,Evidence="WP:123"});
using var doc=JsonDocument.Parse(payload);
if (doc.RootElement.GetProperty("base").GetDecimal()!=100 || doc.RootElement.TryGetProperty("Base",out _)) throw new Exception("Oracle JSON contract mismatch");
Console.WriteLine($"PASS: {checks} validation cases, authority exclusion and Oracle JSON names.");
