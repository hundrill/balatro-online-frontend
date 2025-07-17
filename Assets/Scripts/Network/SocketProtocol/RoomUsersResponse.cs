using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class RoomUsersResponse : BaseSocket
{
    [Serializable]
    public class User
    {
        public string userId;
        public string nickname;
        public int silverChip;
        public int goldChip;
    }

    public override string EventName => EventNameConst;
    public const string EventNameConst = "RoomUsersResponse";

    [SerializeField] private List<User> _users;

    // 프로퍼티로 접근
    public List<User> users => _users;

    public static RoomUsersResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new RoomUsersResponse();

                // success와 message는 BaseSocket에서 처리
                if (dict.TryGetValue("success", out var successObj))
                    response.success = Convert.ToBoolean(successObj);
                if (dict.TryGetValue("message", out var messageObj))
                    response.message = messageObj.ToString();

                // users 파싱
                if (dict.TryGetValue("users", out var usersObj))
                {
                    response._users = new List<User>();

                    if (usersObj is List<object> usersList)
                    {
                        foreach (var userObj in usersList)
                        {
                            if (userObj is Dictionary<string, object> userDict)
                            {
                                var user = new User
                                {
                                    userId = userDict.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "",
                                    nickname = userDict.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "",
                                    silverChip = userDict.TryGetValue("silverChip", out var silverObj) ? Convert.ToInt32(silverObj) : 0,
                                    goldChip = userDict.TryGetValue("goldChip", out var goldObj) ? Convert.ToInt32(goldObj) : 0
                                };
                                response._users.Add(user);
                            }
                        }
                    }
                    else if (usersObj is object[] usersArray)
                    {
                        foreach (var userObj in usersArray)
                        {
                            if (userObj is Dictionary<string, object> userDict)
                            {
                                var user = new User
                                {
                                    userId = userDict.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "",
                                    nickname = userDict.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "",
                                    silverChip = userDict.TryGetValue("silverChip", out var silverObj) ? Convert.ToInt32(silverObj) : 0,
                                    goldChip = userDict.TryGetValue("goldChip", out var goldObj) ? Convert.ToInt32(goldObj) : 0
                                };
                                response._users.Add(user);
                            }
                        }
                    }
                }

                Debug.Log($"[RoomUsersResponse] FromPayload 성공 - users count: {response._users?.Count ?? 0}");
                if (response._users != null)
                {
                    foreach (var user in response._users)
                    {
                        Debug.Log($"[RoomUsersResponse] User: {user.userId} ({user.nickname}) - Silver: {user.silverChip}, Gold: {user.goldChip}");
                    }
                }
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RoomUsersResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}