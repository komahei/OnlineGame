using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{

    public enum Role
    {
        CHASER,
        ESCAPER,
		ONLINE_CHASER,
		ONLINE_ESCAPER,
    }

    public class CharaRole
    {
		private Role charaRole;

		public void setCharaRole(Role role)
		{
			this.charaRole = role;
		}

		public Role getCharaRole()
		{
			return charaRole;
		}
	}

    public class CharacterRole : MonoBehaviour
    {

        private CharaRole charaRole = new CharaRole();


		void Start()
        {
            /*
			charaRole.setCharaRole(Role.ESCAPER);
			Role role = charaRole.getCharaRole();
            // ここのプログラムはコントローラーに移動
            // 生成後にCharaNetManagerからRoleを渡して
            // キャラの大きさやパラメータなどを設定
            if (role == Role.CHASER || role == Role.ONLINE_CHASER)
            {
                this.transform.localScale = new Vector3(7.50f, 7.50f, 7.50f);
            }else if (role == Role.ESCAPER || role == Role.ONLINE_ESCAPER)
            {
				this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			}
            */
        }

    }
}
