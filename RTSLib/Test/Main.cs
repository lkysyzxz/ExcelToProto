using ActorConfig;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSLib.Test
{
	public class Main
	{
		public static void Test()
		{
			Actor actor = new Actor();
			actor.Id = 1;
			actor.SexId = Actor.Types.Sex.Male;
			actor.Name = "wwt";
			actor.SkillType = "magic";

			Actor_Array array = new Actor_Array();

			array.Config.Add(actor);

			object actorobj = actor;

			MessageExtensions.ToByteArray(array as IMessage);


		}
	}
}
