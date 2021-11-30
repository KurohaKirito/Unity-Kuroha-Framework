# 说明

这是一个自己一边学习一边编写的一个的自用的简单游戏框架, 算是一个学习笔记吧

## 泛型单例

使用方法: 

1. 需要实现单例的类继承 `Singleton<T>`, 比如下面的 UIManager:

```csharp
public class UIManager : Singleton<UIManager>
{
    // ...
}
```

2. 在自己的单例类中添加一个单例访问属性, 还是以 UIManager 为例

```csharp
public class UIManager : Singleton<UIManager>
{
    /// <summary>
    /// 单例
    /// </summary>
    public static UIManager Instance => InstanceBase as UIManager;
}
```

3. <font color='red'>注意</font> 如果需要在 `OnDestroy` 放方法中调用单例, 需要在使用之前进行单例活动状态判断, 否则可能会出现 `Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)` 错误.

```csharp
if (UIManager.IsActive)
{
    // 调用单例
    // UIManager.Instance.
    
    // ...
}
```

## 全局消息系统

使用方法:

1. 实现当前事件要用的消息类, 需要继承自 `BaseMessage`, 并将消息要用到的参数添加到类中.

下面是全局帧更新器的消息类, 这个消息类的监听事件目前只需要一个时间增量就可以了, 所以消息类中只有这一个变量.

这个类的本质目的就是把消息的参数进行打包, 而不是写很多的方法来分别对应 1 参事件, 2 参事件, 3 参事件等.

```csharp
public class UpdateMessage : BaseMessage
{
    /// <summary>
    /// 时间增量
    /// </summary>
    public float deltaTime;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="delta"></param>
    public UpdateMessage(float delta)
    {
        deltaTime = delta;
    }
}
```

2. 定义好消息之后, 需要定义消息对应的处理方法, 处理方法要求: 返回值为 `bool`, 参数为 `BaseMessage`, 返回值 bool 的含义为: 是否中止后续的监听者的处理, 如果返回 true, 则当前方法处理完之后, 直接终止后续处理.

3. 需要在合适的地方, 给我们之前定义好的消息类型, 注册我们定义好的消息类对应的处理事件, 注册的方法是:

```csharp
// 方法具有返回值, 返回值为注册是否成功
MessageSystem.Instance.Register<之前定义好的消息类型>(之前定义好的消息类对应的处理事件);
```

4. 关闭场景, 需要注销事件

```csharp
// 方法具有返回值, 返回值为注销是否成功
MessageSystem.Instance.Unregister<之前定义好的消息类型>(之前定义好的消息类对应的处理事件)
```

5. 发送消息的方法:

```csharp
MessageSystem.Instance.Send(之前定义好的消息类型的变量);
```

## 全局帧更新器

## 场景异步加载器

可以对场景进行异步加载

## Json 链表, 字典序列化器

## UI 系统

## 泛型对象池系统

## 全局音频管理器

## 工具类

### DebugUtil

### InputUtil

### NullUtil

### ThreadPoolUtil

