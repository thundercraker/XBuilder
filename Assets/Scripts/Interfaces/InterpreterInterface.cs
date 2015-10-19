using UnityEngine;
using System.Collections;

public interface InterpreterInterface {

    void command(float time, string method, string[] args);
    void set(float time, string var, string value);
	
}
