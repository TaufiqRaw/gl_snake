#version 330
#define MAX_KEYPOINTS 100

#define UP 0
#define RIGHT 1
#define DOWN 2
#define LEFT 3

precision mediump float;

struct MoveKeypoint {
    uint from;
    vec2 at;
    float dstHead;
};

uniform float uCircRadius;
uniform float uLength;

uniform MoveKeypoint[MAX_KEYPOINTS] uKeypoints;
uniform uint uKeypointLen;

bool pointInRadius(vec2 pos, vec2 center);
bool pointInBox(vec2 pos, vec2 boxStart, vec2 boxEnd);
vec4 getColor(float fac, float min, float max);


void main() {
    vec2 frag_pos = vec2(gl_FragCoord.x, gl_FragCoord.y);

    float remainLength = uLength; 

    for(int i= int(uKeypointLen)-1; i >= 0; i--) {

        MoveKeypoint current = uKeypoints[i];

        //set next dst
        //TODO: set this to remaining snake length when no next keypoint
        float nextDst = remainLength;
        float max_percent = 0.;
        float min_percent = 0.;
        if(i > 0){
            int nextIdx = i - 1 ;
            nextDst = uKeypoints[nextIdx].dstHead - current.dstHead;
            max_percent = remainLength/uLength;
            remainLength -= nextDst;
            min_percent = remainLength/uLength;
        } else {
            // this is tail
            vec2 tailPos = vec2(0,0);
            switch(current.from){
                case DOWN : tailPos = vec2(current.at.x,current.at.y - nextDst); break;
                case LEFT : tailPos = vec2(current.at.x - nextDst,current.at.y); break;
                case UP : tailPos = vec2(current.at.x,current.at.y + nextDst); break;
                case RIGHT : tailPos = vec2(current.at.x + nextDst,current.at.y); break;
            }
            if(pointInRadius(frag_pos, tailPos)){
                gl_FragColor = getColor(0., 0., 0.);
                return;
            }
            max_percent = remainLength/uLength;
            min_percent = 0.;
        };

        float curPos = 0.;

        switch(current.from){
            case UP :
                if(pointInBox(frag_pos, 
                    vec2(current.at.x - uCircRadius, current.at.y), 
                    vec2(current.at.x + uCircRadius, current.at.y + nextDst))) {
                        curPos = 1-((frag_pos.y - current.at.y) / nextDst);
                        gl_FragColor = getColor(curPos, min_percent, max_percent);
                        return;
                }
                break;
            case RIGHT : 
                if(pointInBox(frag_pos, 
                    vec2(current.at.x, current.at.y - uCircRadius), 
                    vec2(current.at.x + nextDst, current.at.y + uCircRadius))) {
                        curPos = 1-((frag_pos.x - current.at.x) / nextDst);
                        gl_FragColor = getColor(curPos, min_percent, max_percent);
                        return;
                }
                break;
            case DOWN : 
                if(pointInBox(frag_pos, 
                    vec2(current.at.x - uCircRadius, current.at.y - nextDst), 
                    vec2(current.at.x + uCircRadius, current.at.y))) {
                        curPos = 1-((current.at.y - frag_pos.y) / nextDst);
                        gl_FragColor = getColor(curPos, min_percent, max_percent);
                        return;
                }
                break;
            case LEFT : 
                if(pointInBox(frag_pos, 
                    vec2(current.at.x - nextDst, current.at.y - uCircRadius), 
                    vec2(current.at.x, current.at.y + uCircRadius))) {
                        curPos = 1-((current.at.x - frag_pos.x) / nextDst);
                        gl_FragColor = getColor(curPos, min_percent, max_percent);
                        return;
                }
                break;
        }

        if(pointInRadius(frag_pos, current.at)){
            gl_FragColor = getColor(1-curPos, min_percent, max_percent);
            return;
        }
    };
    discard;  
}

bool pointInRadius(vec2 pos, vec2 center) {
    return (length(pos - center) < uCircRadius);
}

// box start must be on bottom left, box's end must be on top right
// e.g :
// boxStart = (0.1, 0.1)
// boxEnd = (0.5, 0.5)
bool pointInBox(vec2 pos, vec2 boxStart, vec2 boxEnd) {
    return 
        (pos.x > boxStart.x && pos.x < boxEnd.x) &&
        (pos.y > boxStart.y && pos.y < boxEnd.y);
}

vec4 getColor(float fac, float min, float max){
    return vec4(clamp(fac, min, max),  1., clamp(fac, min, max), 1.0);
}