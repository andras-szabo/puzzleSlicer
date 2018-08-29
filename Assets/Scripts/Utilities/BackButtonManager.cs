using System.Collections.Generic;
using UnityEngine;

public class BackButtonManager : MonoSingleton<BackButtonManager>
{
	private struct Callback
	{
		public readonly System.Action call;
		public readonly uint id;
		public readonly bool removeFromStackAfterCalled;

		public Callback(System.Action call, uint id, bool removeFromStackAfterCalled)
		{
			this.call = call;
			this.id = id;
			this.removeFromStackAfterCalled = removeFromStackAfterCalled;
		}
	}

	private Stack<Callback> _backButtonStack = new Stack<Callback>();
	private int _stackCount = 0;
	private uint _UID = 1;
	private bool _isSuspended;

	public void Suspend()
	{
		_isSuspended = true;
	}

	public void Resume()
	{
		_isSuspended = false;
	}

	public uint PushAndGetBackButtonCallbackID(System.Action action, bool removeFromStackAfterCalled = true)
	{
		var uid = _UID++;
		_backButtonStack.Push(new Callback(action, uid, removeFromStackAfterCalled));
		_stackCount = _backButtonStack.Count;
		return uid;
	}

	public void Pop(uint callbackID)
	{
		if (_backButtonStack.Count > 0 && _backButtonStack.Peek().id == callbackID)
		{
			_backButtonStack.Pop();
			_stackCount = _backButtonStack.Count;
		}
	}

	private void Update()
	{
		if (!_isSuspended && _stackCount > 0 && Input.GetKeyDown(KeyCode.Escape))
		{
			var onBackButtonTap = _backButtonStack.Peek();

			if (onBackButtonTap.removeFromStackAfterCalled)
			{
				onBackButtonTap = _backButtonStack.Pop();
			}

			try
			{
				onBackButtonTap.call();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e.Message);
			}
		}
	}
}
