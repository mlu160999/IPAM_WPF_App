using IPAM_WPF_App.Models;

namespace IPAM_WPF_App.Services;

/// <summary>
/// Enthält die Regel, in welcher Reihenfolge der Status einer IP-Adresse wechselt.
/// </summary>
public static class IPStatusWorkflow
{
	/// <summary>
	/// Bestimmt den nächsten Status im Workflow Frei, Reserviert, Zugewiesen und wieder Frei.
	/// </summary>
	/// <param name="aktuellerStatus">Der aktuelle Status der IP-Adresse.</param>
	/// <returns>Der Status nach dem Weiterschalten.</returns>
	public static IPStatus GetNextStatus(IPStatus aktuellerStatus)
	{
		return aktuellerStatus switch
		{
			IPStatus.Frei => IPStatus.Reserviert,
			IPStatus.Reserviert => IPStatus.Zugewiesen,
			IPStatus.Zugewiesen => IPStatus.Frei,
			_ => throw new ArgumentOutOfRangeException(
				nameof(aktuellerStatus), aktuellerStatus, "Unbekannter Status.")
		};
	}
}
