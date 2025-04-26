
using MakerCheckerBasicSampleProject.Models.State;

namespace MakerCheckerBasicSampleProject.StateFilter;

public class DefaultStateFilterFactory
{
	public static Dictionary<Models.State.StateFilter, StateConditionPair> GetStateFilter()
	{
		return new Dictionary<Models.State.StateFilter, StateConditionPair>()
				{
					{new Models.State.StateFilter() { Order = 10, Id = 1, Name = "Active Records"  }, new StateConditionPair { ConditionFlag = 6, ResultFlag = 2 } },
					{new Models.State.StateFilter() { Order = 20, Id = 2, Name = "All Records"  }, new StateConditionPair { ConditionFlag = 0, ResultFlag = 0 } },
					{new Models.State.StateFilter() { Order = 30, Id = 3, Name = "Unverified Records"  } , new StateConditionPair { ConditionFlag = 3, ResultFlag = 0 } },
					{new Models.State.StateFilter() { Order = 40, Id = 4, Name = "Verified Records" }, new StateConditionPair { ConditionFlag = 2, ResultFlag = 2 } },
					{new Models.State.StateFilter() { Order = 50, Id = 5, Name = "Rejected Records"  }, new StateConditionPair { ConditionFlag = 1, ResultFlag = 1 } }
				};
	}
}
