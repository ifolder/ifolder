using System;

namespace Simias.Domain
{
	/// <summary>
	/// Simias Domain Service Interface
	/// </summary>
	public interface IDomainService
	{
		Uri CreateMaster(string id, string name, string owner);
	}
}
