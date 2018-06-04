
using UnityEngine;
using System;

public class FlowHeap {

  private FlowRoutine[] _array = new FlowRoutine[4];
  private int _count = 0;

  public int Count {
    get {
      return _count;
    }
  }

  public void Insert(FlowRoutine element) {
    //if the array isn't big enough, expand it
    if (_array.Length == _count) {
      FlowRoutine[] newArray = new FlowRoutine[_array.Length * 2];
      Array.Copy(_array, newArray, _array.Length);
      _array = newArray;
    }

    element.HeapIndex = _count;
    _count++;

    bubbleUp(element);
  }

  public void Remove(FlowRoutine element) {
    removeAt(element.HeapIndex);
  }

  public FlowRoutine PeekMin() {
    if (_count == 0) {
      throw new Exception("Cannot peek when there are zero elements!");
    }

    return _array[0];
  }

  public FlowRoutine RemoveMin() {
    if (_count == 0) {
      throw new Exception("Cannot Remove Min when there are zero elements!");
    }

    return removeAt(0);
  }

  private FlowRoutine removeAt(int index) {
    FlowRoutine ret = _array[index];
    _count--;

    if (_count == 0) {
      return ret;
    }

    var bottom = _array[_count];
    bottom.HeapIndex = index;

    int parentIndex = getParentIndex(index);
    if (isValidIndex(parentIndex) && _array[parentIndex].HeapValue > bottom.HeapValue) {
      bubbleUp(bottom);
    } else {
      bubbleDown(bottom);
    }

    return ret;
  }

  private void bubbleUp(FlowRoutine element) {
    while (true) {
      if (element.HeapIndex == 0) {
        break;
      }

      int parentIndex = getParentIndex(element.HeapIndex);
      var parent = _array[parentIndex];

      if (parent.HeapValue <= element.HeapValue) {
        break;
      }

      parent.HeapIndex = element.HeapIndex;
      _array[element.HeapIndex] = parent;

      element.HeapIndex = parentIndex;
    }

    _array[element.HeapIndex] = element;
  }

  private void bubbleDown(FlowRoutine element) {
    int elementIndex = element.HeapIndex;
    float elementValue = element.HeapValue;

    while (true) {
      int leftIndex = getChildLeftIndex(elementIndex);
      int rightIndex = getChildRightIndex(elementIndex);

      FlowRoutine smallest = element;
      float smallestValue = elementValue;
      int smallestIndex = elementIndex;

      if (isValidIndex(leftIndex)) {
        var leftChild = _array[leftIndex];
        float leftValue = leftChild.HeapValue;
        if (leftValue < smallestValue) {
          smallest = leftChild;
          smallestIndex = leftIndex;
          smallestValue = leftValue;
        }
      } else {
        break;
      }

      if (isValidIndex(rightIndex)) {
        var rightChild = _array[rightIndex];
        float rightValue = rightChild.HeapValue;
        if (rightValue < smallestValue) {
          smallest = rightChild;
          smallestIndex = rightIndex;
        }
      }

      if (smallestIndex == elementIndex) {
        break;
      }

      smallest.HeapIndex = elementIndex;
      _array[elementIndex] = smallest;

      elementIndex = smallestIndex;
    }

    element.HeapIndex = elementIndex;
    _array[elementIndex] = element;
  }

  private void validateHeap(string operation) {
    for (int i = 0; i < _count; i++) {
      if (_array[i].HeapIndex != i) {
        Debug.LogError("Element " + i + " had an index of " + _array[i].HeapIndex + " instead, after " + operation);
      }
      if (!_array[i].IsValid) {
        Debug.LogError("Element " + i + " was invalid after " + operation);
      }

      if (i != 0) {
        var parent = _array[getParentIndex(i)];
        if (parent.HeapValue > _array[i].HeapValue) {
          Debug.LogError("Element " + i + " had an incorrect order after " + operation);
        }
      }
    }
  }

  private static int getChildLeftIndex(int index) {
    return index * 2 + 1;
  }

  private static int getChildRightIndex(int index) {
    return index * 2 + 2;
  }

  private static int getParentIndex(int index) {
    return (index - 1) / 2;
  }

  private bool isValidIndex(int index) {
    return index < _count && index >= 0;
  }
}
