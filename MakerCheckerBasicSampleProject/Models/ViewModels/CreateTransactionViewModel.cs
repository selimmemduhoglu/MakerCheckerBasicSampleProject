using System.ComponentModel.DataAnnotations;

namespace MakerCheckerBasicSampleProject.Models.ViewModels;

public class CreateTransactionViewModel
{
	[Required(ErrorMessage = "Description is required")]
	[StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
	public string Description { get; set; }

	[Required(ErrorMessage = "Amount is required")]
	[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
	[DataType(DataType.Currency)]
	[Display(Name = "Amount")]
	public decimal Amount { get; set; }

	[Display(Name = "Approval Workflow")]
	public int WorkflowId { get; set; } = 1; // Default to standard workflow
}
