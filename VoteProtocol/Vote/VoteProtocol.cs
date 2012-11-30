#pragma warning disable 1591 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: VoteProtocol.proto
// Note: requires additional types generated from: Protocol.inc.proto
// Note: requires additional types generated from: Vote.inc.proto
// Note: requires additional types generated from: Shogi.inc.proto
namespace VoteSystem.Protocol.Vote
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoteRoomCountRequest")]
  public partial class GetVoteRoomCountRequest : global::ProtoBuf.IExtensible
  {
    public GetVoteRoomCountRequest() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoteRoomCountResponse")]
  public partial class GetVoteRoomCountResponse : global::ProtoBuf.IExtensible
  {
    public GetVoteRoomCountResponse() {}
    
    private int _Count;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Count", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int Count
    {
      get { return _Count; }
      set { _Count = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoteRoomListRequest")]
  public partial class GetVoteRoomListRequest : global::ProtoBuf.IExtensible
  {
    public GetVoteRoomListRequest() {}
    
    private int _FromIndex;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"FromIndex", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int FromIndex
    {
      get { return _FromIndex; }
      set { _FromIndex = value; }
    }
    private int _ToIndex;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"ToIndex", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int ToIndex
    {
      get { return _ToIndex; }
      set { _ToIndex = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoteRoomListResponse")]
  public partial class GetVoteRoomListResponse : global::ProtoBuf.IExtensible
  {
    public GetVoteRoomListResponse() {}
    
    private readonly global::System.Collections.Generic.List<VoteSystem.Protocol.Vote.VoteRoomInfo> _RoomInfoList = new global::System.Collections.Generic.List<VoteSystem.Protocol.Vote.VoteRoomInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"RoomInfoList", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<VoteSystem.Protocol.Vote.VoteRoomInfo> RoomInfoList
    {
      get { return _RoomInfoList; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SetParticipantAttributeRequest")]
  public partial class SetParticipantAttributeRequest : global::ProtoBuf.IExtensible
  {
    public SetParticipantAttributeRequest() {}
    

    private VoteSystem.Protocol.BoolObject _IsUseAsNicoCommenter = null;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"IsUseAsNicoCommenter", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public VoteSystem.Protocol.BoolObject IsUseAsNicoCommenter
    {
      get { return _IsUseAsNicoCommenter; }
      set { _IsUseAsNicoCommenter = value; }
    }

    private bool _IsSetLoginType = default(bool);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"IsSetLoginType", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool IsSetLoginType
    {
      get { return _IsSetLoginType; }
      set { _IsSetLoginType = value; }
    }

    private VoteSystem.Protocol.Vote.NicoLoginType _LoginType = VoteSystem.Protocol.Vote.NicoLoginType.NotLogined;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"LoginType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(VoteSystem.Protocol.Vote.NicoLoginType.NotLogined)]
    public VoteSystem.Protocol.Vote.NicoLoginType LoginType
    {
      get { return _LoginType; }
      set { _LoginType = value; }
    }

    private string _Message = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"Message", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Message
    {
      get { return _Message; }
      set { _Message = value; }
    }

    private bool _HasMessage = default(bool);
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"HasMessage", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool HasMessage
    {
      get { return _HasMessage; }
      set { _HasMessage = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SetParticipantAttributeResponse")]
  public partial class SetParticipantAttributeResponse : global::ProtoBuf.IExtensible
  {
    public SetParticipantAttributeResponse() {}
    
    private VoteSystem.Protocol.Vote.VoteParticipantInfo _Info;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Info", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Vote.VoteParticipantInfo Info
    {
      get { return _Info; }
      set { _Info = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LiveOperationRequest")]
  public partial class LiveOperationRequest : global::ProtoBuf.IExtensible
  {
    public LiveOperationRequest() {}
    
    private VoteSystem.Protocol.Vote.LiveOperation _Operation;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Operation", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public VoteSystem.Protocol.Vote.LiveOperation Operation
    {
      get { return _Operation; }
      set { _Operation = value; }
    }
    private VoteSystem.Protocol.LiveData _LiveData;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"LiveData", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.LiveData LiveData
    {
      get { return _LiveData; }
      set { _LiveData = value; }
    }

    private VoteSystem.Protocol.LiveAttribute _Attribute = null;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"Attribute", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public VoteSystem.Protocol.LiveAttribute Attribute
    {
      get { return _Attribute; }
      set { _Attribute = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LiveOperationResponse")]
  public partial class LiveOperationResponse : global::ProtoBuf.IExtensible
  {
    public LiveOperationResponse() {}
    
    private VoteSystem.Protocol.Vote.LiveOperation _Operation;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Operation", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public VoteSystem.Protocol.Vote.LiveOperation Operation
    {
      get { return _Operation; }
      set { _Operation = value; }
    }
    private VoteSystem.Protocol.LiveData _LiveData;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"LiveData", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.LiveData LiveData
    {
      get { return _LiveData; }
      set { _LiveData = value; }
    }
    private VoteSystem.Protocol.LiveAttribute _Attribute;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"Attribute", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.LiveAttribute Attribute
    {
      get { return _Attribute; }
      set { _Attribute = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"DisconnectRequest")]
  public partial class DisconnectRequest : global::ProtoBuf.IExtensible
  {
    public DisconnectRequest() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"DisconnectResponse")]
  public partial class DisconnectResponse : global::ProtoBuf.IExtensible
  {
    public DisconnectResponse() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CreateVoteRoomRequest")]
  public partial class CreateVoteRoomRequest : global::ProtoBuf.IExtensible
  {
    public CreateVoteRoomRequest() {}
    
    private string _RoomName;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"RoomName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string RoomName
    {
      get { return _RoomName; }
      set { _RoomName = value; }
    }

    private string _Password = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"Password", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Password
    {
      get { return _Password; }
      set { _Password = value; }
    }
    private string _OwnerName;
    [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name=@"OwnerName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string OwnerName
    {
      get { return _OwnerName; }
      set { _OwnerName = value; }
    }
    private string _ImageUrl;
    [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name=@"ImageUrl", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ImageUrl
    {
      get { return _ImageUrl; }
      set { _ImageUrl = value; }
    }
    private string _OwnerId;
    [global::ProtoBuf.ProtoMember(12, IsRequired = true, Name=@"OwnerId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string OwnerId
    {
      get { return _OwnerId; }
      set { _OwnerId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CreateVoteRoomResponse")]
  public partial class CreateVoteRoomResponse : global::ProtoBuf.IExtensible
  {
    public CreateVoteRoomResponse() {}
    
    private VoteSystem.Protocol.Vote.VoteRoomInfo _RoomInfo;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"RoomInfo", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Vote.VoteRoomInfo RoomInfo
    {
      get { return _RoomInfo; }
      set { _RoomInfo = value; }
    }
    private int _ParticipantNo;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"ParticipantNo", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int ParticipantNo
    {
      get { return _ParticipantNo; }
      set { _ParticipantNo = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"EnterVoteRoomRequest")]
  public partial class EnterVoteRoomRequest : global::ProtoBuf.IExtensible
  {
    public EnterVoteRoomRequest() {}
    
    private int _RoomId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"RoomId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int RoomId
    {
      get { return _RoomId; }
      set { _RoomId = value; }
    }

    private string _Password = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"Password", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Password
    {
      get { return _Password; }
      set { _Password = value; }
    }
    private string _ParticipantName;
    [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name=@"ParticipantName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ParticipantName
    {
      get { return _ParticipantName; }
      set { _ParticipantName = value; }
    }
    private string _ImageUrl;
    [global::ProtoBuf.ProtoMember(11, IsRequired = true, Name=@"ImageUrl", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ImageUrl
    {
      get { return _ImageUrl; }
      set { _ImageUrl = value; }
    }
    private string _ParticipantId;
    [global::ProtoBuf.ProtoMember(12, IsRequired = true, Name=@"ParticipantId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ParticipantId
    {
      get { return _ParticipantId; }
      set { _ParticipantId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"EnterVoteRoomResponse")]
  public partial class EnterVoteRoomResponse : global::ProtoBuf.IExtensible
  {
    public EnterVoteRoomResponse() {}
    
    private VoteSystem.Protocol.Vote.VoteRoomInfo _RoomInfo;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"RoomInfo", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Vote.VoteRoomInfo RoomInfo
    {
      get { return _RoomInfo; }
      set { _RoomInfo = value; }
    }
    private int _ParticipantNo;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"ParticipantNo", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int ParticipantNo
    {
      get { return _ParticipantNo; }
      set { _ParticipantNo = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoterListRequest")]
  public partial class GetVoterListRequest : global::ProtoBuf.IExtensible
  {
    public GetVoterListRequest() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetVoterListResponse")]
  public partial class GetVoterListResponse : global::ProtoBuf.IExtensible
  {
    public GetVoterListResponse() {}
    
    private VoteSystem.Protocol.Vote.VoterList _VoterList;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"VoterList", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Vote.VoterList VoterList
    {
      get { return _VoterList; }
      set { _VoterList = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LeaveVoteRoomRequest")]
  public partial class LeaveVoteRoomRequest : global::ProtoBuf.IExtensible
  {
    public LeaveVoteRoomRequest() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LeaveVoteRoomResponse")]
  public partial class LeaveVoteRoomResponse : global::ProtoBuf.IExtensible
  {
    public LeaveVoteRoomResponse() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SendVoteResultCommand")]
  public partial class SendVoteResultCommand : global::ProtoBuf.IExtensible
  {
    public SendVoteResultCommand() {}
    
    private VoteSystem.Protocol.Vote.VoteResult _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Vote.VoteResult Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ChangeVoteModeCommand")]
  public partial class ChangeVoteModeCommand : global::ProtoBuf.IExtensible
  {
    public ChangeVoteModeCommand() {}
    
    private VoteSystem.Protocol.Vote.VoteMode _VoteMode;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"VoteMode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public VoteSystem.Protocol.Vote.VoteMode VoteMode
    {
      get { return _VoteMode; }
      set { _VoteMode = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"StartVoteCommand")]
  public partial class StartVoteCommand : global::ProtoBuf.IExtensible
  {
    public StartVoteCommand() {}
    

    private double _Seconds = default(double);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"Seconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(double))]
    public double Seconds
    {
      get { return _Seconds; }
      set { _Seconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"PauseVoteCommand")]
  public partial class PauseVoteCommand : global::ProtoBuf.IExtensible
  {
    public PauseVoteCommand() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"StopVoteCommand")]
  public partial class StopVoteCommand : global::ProtoBuf.IExtensible
  {
    public StopVoteCommand() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SetVoteSpanCommand")]
  public partial class SetVoteSpanCommand : global::ProtoBuf.IExtensible
  {
    public SetVoteSpanCommand() {}
    
    private double _Seconds;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Seconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double Seconds
    {
      get { return _Seconds; }
      set { _Seconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"AddVoteSpanCommand")]
  public partial class AddVoteSpanCommand : global::ProtoBuf.IExtensible
  {
    public AddVoteSpanCommand() {}
    
    private double _DiffSeconds;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"DiffSeconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double DiffSeconds
    {
      get { return _DiffSeconds; }
      set { _DiffSeconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SetTotalVoteSpanCommand")]
  public partial class SetTotalVoteSpanCommand : global::ProtoBuf.IExtensible
  {
    public SetTotalVoteSpanCommand() {}
    
    private double _Seconds;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Seconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double Seconds
    {
      get { return _Seconds; }
      set { _Seconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"AddTotalVoteSpanCommand")]
  public partial class AddTotalVoteSpanCommand : global::ProtoBuf.IExtensible
  {
    public AddTotalVoteSpanCommand() {}
    
    private double _DiffSeconds;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"DiffSeconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double DiffSeconds
    {
      get { return _DiffSeconds; }
      set { _DiffSeconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ClearVoteCommand")]
  public partial class ClearVoteCommand : global::ProtoBuf.IExtensible
  {
    public ClearVoteCommand() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"NotificationCommand")]
  public partial class NotificationCommand : global::ProtoBuf.IExtensible
  {
    public NotificationCommand() {}
    
    private VoteSystem.Protocol.Notification _Notification;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Notification", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public VoteSystem.Protocol.Notification Notification
    {
      get { return _Notification; }
      set { _Notification = value; }
    }
    private bool _IsFromLiveOwner;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"IsFromLiveOwner", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public bool IsFromLiveOwner
    {
      get { return _IsFromLiveOwner; }
      set { _IsFromLiveOwner = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"StartEndRollCommand")]
  public partial class StartEndRollCommand : global::ProtoBuf.IExtensible
  {
    public StartEndRollCommand() {}
    
    private double _RollTimeSeconds;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"RollTimeSeconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double RollTimeSeconds
    {
      get { return _RollTimeSeconds; }
      set { _RollTimeSeconds = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"StopEndRollCommand")]
  public partial class StopEndRollCommand : global::ProtoBuf.IExtensible
  {
    public StopEndRollCommand() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ShogiGetCurrentBoardRequest")]
  public partial class ShogiGetCurrentBoardRequest : global::ProtoBuf.IExtensible
  {
    public ShogiGetCurrentBoardRequest() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ShogiGetCurrentBoardResponse")]
  public partial class ShogiGetCurrentBoardResponse : global::ProtoBuf.IExtensible
  {
    public ShogiGetCurrentBoardResponse() {}
    
    private Ragnarok.Shogi.Board _Board;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Board", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public Ragnarok.Shogi.Board Board
    {
      get { return _Board; }
      set { _Board = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ShogiSetCurrentBoardCommand")]
  public partial class ShogiSetCurrentBoardCommand : global::ProtoBuf.IExtensible
  {
    public ShogiSetCurrentBoardCommand() {}
    
    private Ragnarok.Shogi.Board _Board;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Board", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public Ragnarok.Shogi.Board Board
    {
      get { return _Board; }
      set { _Board = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ShogiGetWhaleClientListCommand")]
  public partial class ShogiGetWhaleClientListCommand : global::ProtoBuf.IExtensible
  {
    public ShogiGetWhaleClientListCommand() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ShogiSetWhaleClientListCommand")]
  public partial class ShogiSetWhaleClientListCommand : global::ProtoBuf.IExtensible
  {
    public ShogiSetWhaleClientListCommand() {}
    
    private readonly global::System.Collections.Generic.List<string> _NameList = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(1, Name=@"NameList", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> NameList
    {
      get { return _NameList; }
    }
  
    private double _Value;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Value", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double Value
    {
      get { return _Value; }
      set { _Value = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"NicoLoginType")]
    public enum NicoLoginType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"NotLogined", Value=0)]
      NotLogined = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Normal", Value=1)]
      Normal = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Premium", Value=2)]
      Premium = 2
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"LiveOperation")]
    public enum LiveOperation
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"None", Value=0)]
      None = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Add", Value=1)]
      Add = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Remove", Value=2)]
      Remove = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"SetAttribute", Value=3)]
      SetAttribute = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GetAttribute", Value=4)]
      GetAttribute = 4
    }
  
}