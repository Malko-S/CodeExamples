using Next.Core.Time;
using Next.Core.Utility;
using Next.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Main
{
	public class HotspotFactory : MonoBehaviour
	{
		public GameObject HotspotPrefab;
		private DynamicArray<HotspotControl> _hotspots;

		private void OnEnable()
		{
			_hotspots = new DynamicArray<HotspotControl>( 1, 1 );
			_hotspots.Add( HotspotPrefab.GetComponent<HotspotControl>() );
		}

		public void InitializeHotspots( int contentID, int childCount )
		{
			for( int i = 0; i < childCount; i++ )
			{
				if( _hotspots.Length > i )
					_hotspots[i].Initialize( contentID, i + 1 );
				else
				{
					_hotspots.Add( Instantiate( HotspotPrefab, transform ).GetComponent<HotspotControl>() );
					_hotspots[i].Initialize( contentID, i + 1 );
				}
			}

			for( int i = 0; i < _hotspots.Length; i++ )
			{
				_hotspots[i].Hide();
			}
		}

		public IEnumerator ShowHotspots()
		{
			for( int i = 0; i < _hotspots.Length; i++ )
			{
				_hotspots[i].Show();
				yield return Timing.TimeA.WaitSeconds( 0.05f );
			}
			yield return null;
		}

		public IEnumerator HideHotspots( int last = -1 )
		{
			for( int i = 0; i < _hotspots.Length; i++ )
			{
				if( i != last - 1 || last == -1 )
					_hotspots[i].Hide();
				yield return Timing.TimeA.WaitSeconds( 0.1f );
			}
			yield return Timing.TimeA.WaitSeconds( 0.1f );
			if( last != -1 )
				_hotspots[last - 1].Hide();
			yield return null;
		}
	}
}
