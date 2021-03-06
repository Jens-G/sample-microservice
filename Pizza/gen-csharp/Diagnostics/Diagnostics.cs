/**
 * Autogenerated by Thrift Compiler (1.0.0-dev)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace Diagnostics
{
  public partial class Diagnostics {
    public interface ISync {
      long PerformanceTest(int seconds);
    }

    public interface Iface : ISync {
      #if SILVERLIGHT
      IAsyncResult Begin_PerformanceTest(AsyncCallback callback, object state, int seconds);
      long End_PerformanceTest(IAsyncResult asyncResult);
      #endif
    }

    public class Client : IDisposable, Iface {
      public Client(TProtocol prot) : this(prot, prot)
      {
      }

      public Client(TProtocol iprot, TProtocol oprot)
      {
        iprot_ = iprot;
        oprot_ = oprot;
      }

      protected TProtocol iprot_;
      protected TProtocol oprot_;
      protected int seqid_;

      public TProtocol InputProtocol
      {
        get { return iprot_; }
      }
      public TProtocol OutputProtocol
      {
        get { return oprot_; }
      }


      #region " IDisposable Support "
      private bool _IsDisposed;

      // IDisposable
      public void Dispose()
      {
        Dispose(true);
      }
      

      protected virtual void Dispose(bool disposing)
      {
        if (!_IsDisposed)
        {
          if (disposing)
          {
            if (iprot_ != null)
            {
              ((IDisposable)iprot_).Dispose();
            }
            if (oprot_ != null)
            {
              ((IDisposable)oprot_).Dispose();
            }
          }
        }
        _IsDisposed = true;
      }
      #endregion


      
      #if SILVERLIGHT
      public IAsyncResult Begin_PerformanceTest(AsyncCallback callback, object state, int seconds)
      {
        return send_PerformanceTest(callback, state, seconds);
      }

      public long End_PerformanceTest(IAsyncResult asyncResult)
      {
        oprot_.Transport.EndFlush(asyncResult);
        return recv_PerformanceTest();
      }

      #endif

      public long PerformanceTest(int seconds)
      {
        #if !SILVERLIGHT
        send_PerformanceTest(seconds);
        return recv_PerformanceTest();

        #else
        var asyncResult = Begin_PerformanceTest(null, null, seconds);
        return End_PerformanceTest(asyncResult);

        #endif
      }
      #if SILVERLIGHT
      public IAsyncResult send_PerformanceTest(AsyncCallback callback, object state, int seconds)
      #else
      public void send_PerformanceTest(int seconds)
      #endif
      {
        oprot_.WriteMessageBegin(new TMessage("PerformanceTest", TMessageType.Call, seqid_));
        PerformanceTest_args args = new PerformanceTest_args();
        args.Seconds = seconds;
        args.Write(oprot_);
        oprot_.WriteMessageEnd();
        #if SILVERLIGHT
        return oprot_.Transport.BeginFlush(callback, state);
        #else
        oprot_.Transport.Flush();
        #endif
      }

      public long recv_PerformanceTest()
      {
        TMessage msg = iprot_.ReadMessageBegin();
        if (msg.Type == TMessageType.Exception) {
          TApplicationException x = TApplicationException.Read(iprot_);
          iprot_.ReadMessageEnd();
          throw x;
        }
        PerformanceTest_result result = new PerformanceTest_result();
        result.Read(iprot_);
        iprot_.ReadMessageEnd();
        if (result.__isset.success) {
          return result.Success;
        }
        if (result.__isset.error) {
          throw result.Error;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "PerformanceTest failed: unknown result");
      }

    }
    public class Processor : TProcessor {
      public Processor(ISync iface)
      {
        iface_ = iface;
        processMap_["PerformanceTest"] = PerformanceTest_Process;
      }

      protected delegate void ProcessFunction(int seqid, TProtocol iprot, TProtocol oprot);
      private ISync iface_;
      protected Dictionary<string, ProcessFunction> processMap_ = new Dictionary<string, ProcessFunction>();

      public bool Process(TProtocol iprot, TProtocol oprot)
      {
        try
        {
          TMessage msg = iprot.ReadMessageBegin();
          ProcessFunction fn;
          processMap_.TryGetValue(msg.Name, out fn);
          if (fn == null) {
            TProtocolUtil.Skip(iprot, TType.Struct);
            iprot.ReadMessageEnd();
            TApplicationException x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            oprot.WriteMessageBegin(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID));
            x.Write(oprot);
            oprot.WriteMessageEnd();
            oprot.Transport.Flush();
            return true;
          }
          fn(msg.SeqID, iprot, oprot);
        }
        catch (IOException)
        {
          return false;
        }
        return true;
      }

      public void PerformanceTest_Process(int seqid, TProtocol iprot, TProtocol oprot)
      {
        PerformanceTest_args args = new PerformanceTest_args();
        args.Read(iprot);
        iprot.ReadMessageEnd();
        PerformanceTest_result result = new PerformanceTest_result();
        try
        {
          try
          {
            result.Success = iface_.PerformanceTest(args.Seconds);
          }
          catch (EDiagnostics error)
          {
            result.Error = error;
          }
          oprot.WriteMessageBegin(new TMessage("PerformanceTest", TMessageType.Reply, seqid)); 
          result.Write(oprot);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          TApplicationException x = new TApplicationException        (TApplicationException.ExceptionType.InternalError," Internal error.");
          oprot.WriteMessageBegin(new TMessage("PerformanceTest", TMessageType.Exception, seqid));
          x.Write(oprot);
        }
        oprot.WriteMessageEnd();
        oprot.Transport.Flush();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class PerformanceTest_args : TBase
    {
      private int _seconds;

      public int Seconds
      {
        get
        {
          return _seconds;
        }
        set
        {
          __isset.seconds = true;
          this._seconds = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool seconds;
      }

      public PerformanceTest_args() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.I32) {
                  Seconds = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("PerformanceTest_args");
          oprot.WriteStructBegin(struc);
          TField field = new TField();
          if (__isset.seconds) {
            field.Name = "seconds";
            field.Type = TType.I32;
            field.ID = 1;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Seconds);
            oprot.WriteFieldEnd();
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that) {
        var other = that as PerformanceTest_args;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.seconds == other.__isset.seconds) && ((!__isset.seconds) || (System.Object.Equals(Seconds, other.Seconds))));
      }

      public override int GetHashCode() {
        int hashcode = 0;
        unchecked {
          hashcode = (hashcode * 397) ^ (!__isset.seconds ? 0 : (Seconds.GetHashCode()));
        }
        return hashcode;
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("PerformanceTest_args(");
        bool __first = true;
        if (__isset.seconds) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Seconds: ");
          __sb.Append(Seconds);
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class PerformanceTest_result : TBase
    {
      private long _success;
      private EDiagnostics _error;

      public long Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public EDiagnostics Error
      {
        get
        {
          return _error;
        }
        set
        {
          __isset.error = true;
          this._error = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool success;
        public bool error;
      }

      public PerformanceTest_result() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.I64) {
                  Success = iprot.ReadI64();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              case 1:
                if (field.Type == TType.Struct) {
                  Error = new EDiagnostics();
                  Error.Read(iprot);
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("PerformanceTest_result");
          oprot.WriteStructBegin(struc);
          TField field = new TField();

          if (this.__isset.success) {
            field.Name = "Success";
            field.Type = TType.I64;
            field.ID = 0;
            oprot.WriteFieldBegin(field);
            oprot.WriteI64(Success);
            oprot.WriteFieldEnd();
          } else if (this.__isset.error) {
            if (Error != null) {
              field.Name = "Error";
              field.Type = TType.Struct;
              field.ID = 1;
              oprot.WriteFieldBegin(field);
              Error.Write(oprot);
              oprot.WriteFieldEnd();
            }
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override bool Equals(object that) {
        var other = that as PerformanceTest_result;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
          && ((__isset.error == other.__isset.error) && ((!__isset.error) || (System.Object.Equals(Error, other.Error))));
      }

      public override int GetHashCode() {
        int hashcode = 0;
        unchecked {
          hashcode = (hashcode * 397) ^ (!__isset.success ? 0 : (Success.GetHashCode()));
          hashcode = (hashcode * 397) ^ (!__isset.error ? 0 : (Error.GetHashCode()));
        }
        return hashcode;
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("PerformanceTest_result(");
        bool __first = true;
        if (__isset.success) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Success: ");
          __sb.Append(Success);
        }
        if (Error != null && __isset.error) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Error: ");
          __sb.Append(Error== null ? "<null>" : Error.ToString());
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }

  }
}
