"""alter_class.py

A filter script expecting a command line of the form:

./alter_class.py L C

Where L is a new location ID and C is a new class ID. Given a location L
and a class C:

    * Read turns from stdin
    * Change the turns' location and class to L and C, respectively
    * Write the modified turns to stdout

Note that the format used is the same format (one JSON record per line) used
by turn_copy.py for the text file format
"""

import sys
import json
import functools

def usage():
    print __doc__
    return 1

def parse_user_id(userid):
    """Given a 'full' user ID, parse out and return the location ID, class
    ID, and user ID.  Note that the 'full' user ID might also be 'simple'
    meaning that it is JUST a user ID.  See CSALDatabase.SaveRawStudentLessonAct
    in the CSALMongo project for the canonical implementation of this logic.
    """
    #Default to assuming that userid is JUST a user ID
    loc, cls, usr = None, None, userid
    
    fields = userid.split('-') if userid else []
    if len(fields) > 2:
        loc = fields[0]
        cls = fields[1]
        usr = '-'.join(fields[2:])
    
    return loc, cls, usr


def xform_turn(new_location, new_clazz, turn):
    """Change the given turn to have the specified new location and class.
    Note that this information is currently encoded in the User ID field
    """
    old_location, old_clazz, user_id = parse_user_id(turn.get("UserID", ""))
    if not user_id:
        raise ValueError("No User ID in turn data")
 
    turn["UserID"] = '-'.join([new_location, new_clazz, user_id])
    return turn


def main():
    args = sys.argv[1:]
    
    if len(args) != 2:
        return usage()
    
    location, clazz = args
    
    #Parse all input before writing output - that way we catch any errors
    #before writing output
    turns = []
    for line in sys.stdin:
        if line.strip():
            turns.append(json.loads(line))
    
    #Simply code by composing a new xformation based invariant parameters
    xf = functools.partial(xform_turn, location, clazz)
    
    #Actually write out changes
    for turn in turns:
        print json.dumps(xf(turn))

if __name__ == "__main__":
    main()
